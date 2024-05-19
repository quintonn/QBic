export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
}

const API_VERSION = "v1";

let appVersion = ""; //TODO: make a hook/state ??

let baseURL = "";
let apiUrl = "";

export const initializeSystem = async (): Promise<SystemInfo> => {
  const scheme = window.location.protocol; //"https";
  baseURL = scheme + "//" + window.location.host + window.location.pathname;
  //console.log("base url = " + baseURL);
  //console.log("root url = " + process.env.ROOT_URL);

  if (process.env.ROOT_URL) {
    //console.log("dev root url is not empty");
    baseURL = process.env.ROOT_URL;
  }

  if (!baseURL.endsWith("/")) {
    baseURL += "/";
  }

  apiUrl = `${baseURL}api/${API_VERSION}/`;

  // TODO: Move all of the above to a hook or something (maybe just a file that gets imported by app.ts);

  const url = `initializeSystem`;

  const systemInfo = await makeApiCall<SystemInfo>(url, "GET");

  if (systemInfo) {
    if (systemInfo.ConstructionError) {
      console.log("There was an error in the system initialization code:");
      console.log(systemInfo.ConstructionError);
      return null;
    }
    appVersion = systemInfo.Version;
    document.title = `${systemInfo.ApplicationName} ${systemInfo.Version}`;
  }

  return systemInfo;
};

export const makeApiCall = async <T extends any>(
  url: string,
  method: "GET" | "POST" = "GET",
  data?: any,
  raiseErrors: boolean = true
): Promise<T> => {
  // Setup
  let cacheControl = `&_=${Date.now()}`; // don't cache stuff
  if (url.includes("html")) {
    cacheControl = "";
  }
  const urlToCall = `${apiUrl}${url}?v=${appVersion}${cacheControl}`;

  // make API call
  try {
    const fetchOptions: RequestInit = {
      method: method,
    };

    if (method == "POST") {
      fetchOptions.body = JSON.stringify(data);
    }

    const response = await fetch(urlToCall, fetchOptions);

    if (response.status >= 200 && response.status < 300) {
      // success
      var json = (await response.json()) as T;
      return Promise.resolve(json);
    } else if (response.status == 401) {
      console.log("unauthorized");

      await performTokenRefresh(); //TODO: Api service will have to be a hook to so we can use this part
      // try freshing the token and re-doing the api call
    } else {
      alert("Unhandled response status: " + response.status);
      return null;
      //TODO: handle response
    }
  } catch (err) {
    console.log("error making api call to: " + urlToCall);
    console.log(err);
    if (raiseErrors == true) {
      //TODO: show these errors somewhere (flashbar);
    } else {
      return null;
    }

    // alert(
    //   "Error contacting the server. You might not have internet or our server is down"
    // );
  }
};

// making an API call.
/*
    If it fails with 401 (not logged in):
        If the refresh token is not empty, try use it to get an access token and try the api call again.
        If this call fails, show the login dialog again

    If 400 and we see an "invalid_grant" in response, this means user entered wrong credentials (this can probably be done elsewhere)

    For other errors, handle the error:
        Get an error message and show it to the user (flashbar?).

    

*/
