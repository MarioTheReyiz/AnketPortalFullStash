// Tüm isteklere token ekle
$.ajaxSetup({
    beforeSend: function (xhr) {
        var token = localStorage.getItem("token");
        if (token) {
            xhr.setRequestHeader('Authorization', 'Bearer ' + token);
        }
    }
});

// Oturum biterse (401) otomatik yenileme yap
$(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
    if (jqXHR.status === 401) {
        var refreshToken = localStorage.getItem("refreshToken");
        if (refreshToken) {
            $.ajax({
                url: "https://localhost:7094/api/Auth/RefreshToken", // Kendi portunla aynı olduğundan emin ol
                type: "POST",
                contentType: "application/json",
                data: JSON.stringify(refreshToken),
                async: false,
                success: function (res) {
                    localStorage.setItem("token", res.data.accessToken);
                    localStorage.setItem("refreshToken", res.data.refreshToken);
                    location.reload(); // Yeni tokenla sayfayı tazele
                },
                error: function () {
                    localStorage.removeItem("token");
                    localStorage.removeItem("refreshToken");
                    window.location.href = "/Auth/Login";
                }
            });
        } else {
            window.location.href = "/Auth/Login";
        }
    }
});