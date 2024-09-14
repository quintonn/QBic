import { addMessage } from "../App/flashbarSlice";
import { store } from "../App/store";
import { API_URL } from "../Constants/AppValues";
import { useAuth } from "../ContextProviders/AuthProvider/AuthProvider";
import { useMainApp } from "../ContextProviders/MainAppProvider/MainAppProvider";

const AUTH_RETRY_ATTEMPTS = 5;

export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
}

export const useApi = () => {
  const auth = useAuth();

  const { appVersion } = useMainApp();

  const makeApiCall = async <T extends any>(
    url: string,
    method: "GET" | "POST" = "GET",
    data?: any
  ): Promise<T> => {
    // Setup
    let cacheControl = `&_=${Date.now()}`; // don't cache stuff
    if (url.includes("html")) {
      cacheControl = "";
    }
    const urlToCall = `${API_URL}${url}?v=${appVersion}${cacheControl}`;

    // make API call
    const fetchOptions: RequestInit = {
      method: method,
      headers: {
        Authorization: "Bearer " + auth.getAccessToken(),
      },
    };
    if (method == "POST") {
      if (data instanceof FormData) {
        fetchOptions.body = data;
      } else {
        fetchOptions.body = JSON.stringify(data);
      }
    }

    return await makeApiCallInternal(urlToCall, fetchOptions);
  };

  const initializeSystem = async (): Promise<SystemInfo> => {
    // let cacheControl = `&_=${Date.now()}`; // don't cache stuff
    // const urlToCall = `${appInfo.apiUrl}initializeSystem?v=${appInfo.appVersion}${cacheControl}`;

    // const fetchOptions: RequestInit = {
    //   method: "GET",
    // };

    const systemInfo = await makeApiCall<SystemInfo>("initializeSystem");
    if (systemInfo) {
      if (systemInfo.ConstructionError) {
        console.log("There was an error in the system initialization code:");
        console.log(systemInfo.ConstructionError);
        return null;
      }
      //appInfo.setAppVersion(systemInfo.Version);
      document.title = `${systemInfo.ApplicationName} ${systemInfo.Version}`;
    }

    return systemInfo;
  };

  const makeApiCallInternal = async <T extends any>(
    urlToCall: string,
    fetchOptions: RequestInit,
    raiseErrors: boolean = true,
    authRetryCount: number = 1
  ): Promise<T> => {
    // make API call
    try {
      const response = await fetch(urlToCall, fetchOptions);
      if (response.ok) {
        // success
        const json = (await response.json()) as T;

        return Promise.resolve(json);
      } else if (response.status == 401) {
        if (authRetryCount > AUTH_RETRY_ATTEMPTS) {
          // only try AUTH_RETRY_ATTEMPTS times
          console.log(
            `auth retry count exceeded ${AUTH_RETRY_ATTEMPTS} attempts`
          );
          store.dispatch(
            addMessage({
              type: "error",
              content: `Unable to authenticate with server, ${AUTH_RETRY_ATTEMPTS} attempts were made`,
            })
          );
          return Promise.reject(
            "Unable to authenticate with server, multiple attempts were made"
          );
        }
        return auth
          .performTokenRefresh()
          .then((x) => {
            // try call again
            fetchOptions.headers["Authorization"] =
              "Bearer " + auth.getAccessToken(); // update auth token

            return makeApiCallInternal<T>(
              urlToCall,
              fetchOptions,
              raiseErrors,
              authRetryCount + 1
            ).then((x) => {
              return Promise.resolve(x as T);
            });
          })
          .catch((err) => {
            console.error("error while getting refresh token");
            console.log(err);
            // TODO: show login dialog -> which should then essentially restart the application initialization stuff as it will have new tokens
            // setShowLoginDialog -> or something like that
            return Promise.resolve(null as T);
          });
      } else if (response.status == 400) {
        let message = await response.text();
        if (message.includes("invalid_grant")) {
          message = "Incorrect username or password";
          // show message that username or password was incorrect
        } else if (message.startsWith('"')) {
          message = message.substring(1, message.length - 2);
        }
        store.dispatch(
          addMessage({
            type: "error",
            content: message,
          })
        );
        return Promise.resolve(null);
      }

      const responseText = await response.text();
      const message = `Unhandled response status ${response.status} with content: ${responseText}`;

      store.dispatch(
        addMessage({
          type: "error",
          content: message,
        })
      );

      return Promise.reject(message);
    } catch (err) {
      console.log("error making api call to: " + urlToCall);
      console.log(err);
      if (raiseErrors == true) {
        store.dispatch(
          addMessage({
            type: "error",
            content: `Error making api call to: ${urlToCall}`,
          })
        );
      } else {
        return null;
      }

      // alert(
      //   "Error contacting the server. You might not have internet or our server is down"
      // );
    }
  };

  return { makeApiCall, initializeSystem };
};
