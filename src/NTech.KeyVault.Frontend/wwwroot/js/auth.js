export async function SignIn(dotNetReference, loginModel) {
    /*try {*/
        const url = new URL(window.location.href);

        const response = await fetch("/auth/login", {
            method: "POST",
            redirect: "manual",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json;charset=UTF-8'
            },
            body: JSON.stringify(loginModel)
        });

        if (response.ok) {
            const result = await response.json();

            if (result.success) {
                dotNetReference.invokeMethodAsync("HandleLoginSuccess");
            } else if (result.isMfaRequired) {
                dotNetReference.invokeMethodAsync("HandleMFARequired", result.mfaInfo);
            } else {
                dotNetReference.invokeMethodAsync("HandleLoginFailed", result.errorMessage);
            }
        }
    //}
    //catch {
    //    dotNetReference.invokeMethodAsync("HandleLoginFailed", "Login has failed.");
    //}
}

export async function Refresh() {
    try {
        const response = await fetch("/auth/refresh", {
            method: "POST",
            redirect: "manual",
            headers: {
                'Accept': 'application/json',
                'Content-Type': 'application/json;charset=UTF-8'
            }
        });
    }
    catch {

    }
}