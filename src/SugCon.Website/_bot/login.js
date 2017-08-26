// Note: Disabling and enabling to make bootstrap field validation work

window.onload = function () {
    document.querySelector("#login input[type='submit']").onclick = function () {
        if (document.getElementById("UserName").value === "" || document.getElementById("Password").value === "") {
            document.getElementById("credentialsError").style.display = "block";
            var loginFailedMessageNode = document.getElementById("loginFailedMessage");
            if (loginFailedMessageNode != null) {
                loginFailedMessageNode.style.display = "none";
            }
            return false;
        } else {
            document.getElementById("credentialsError").style.display = "none";
        }
    };
}