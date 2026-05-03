<script>
    // Tüm sayfalardaki AJAX hatalarını global olarak dinleyen "Interceptor"
    $(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
        
        // Eğer hata 401 (Token süresi bitti) ise VE istek zaten login/refresh değilse:
        if (jqXHR.status === 401 && !ajaxSettings.url.includes("RefreshToken") && !ajaxSettings.url.includes("Login")) {
            
            var currentRefreshToken = localStorage.getItem("refreshToken");

    if (currentRefreshToken) {
        // Kullanıcıya çaktırmadan senin API'ndeki metoda yeni token isteği atıyoruz
        $.ajax({
            url: "https://localhost:7094/api/Auth/RefreshToken",
            type: "POST",
            contentType: "application/json",
            // Senin AuthController "[FromBody] string refreshToken" beklediği için direkt string'i JSON yapıp yolluyoruz:
            data: JSON.stringify(currentRefreshToken),
            success: function (res) {
                if (res.status) {
                    var newAccessToken = res.data.accessToken || res.data.AccessToken;
                    var newRefreshToken = res.data.refreshToken || res.data.RefreshToken;

                    // 1. Yeni gelen tokenları hafızaya güncelle
                    localStorage.setItem("token", newAccessToken);
                    localStorage.setItem("refreshToken", newRefreshToken);

                    // 2. Patlayan orijinal isteğin "Header" kısmına yeni token'ı çak ve TEKRAR ÇALIŞTIR!
                    ajaxSettings.headers = ajaxSettings.headers || {};
                    ajaxSettings.headers["Authorization"] = "Bearer " + newAccessToken;

                    $.ajax(ajaxSettings); // Kullanıcı hiçbir şey hissetmeden işlem gerçekleşir
                } else {
                    // Refresh token'ın da 7 günlük süresi dolmuşsa çıkışa yolla
                    window.location.href = "/Auth/Login";
                }
            },
            error: function () {
                // Refresh token geçersizse/silinmişse çıkışa yolla
                window.location.href = "/Auth/Login";
            }
        });
            } else {
        // Hafızada refresh token bile yoksa direkt çıkış
        window.location.href = "/Auth/Login";
            }
        }
    });
</script>