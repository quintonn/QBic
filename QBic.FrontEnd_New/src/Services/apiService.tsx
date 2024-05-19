export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
}

const API_VERSION = "v1";

let appVersion = ""; //TODO: make a hook/state ??

export const initializeSystem = async (): Promise<SystemInfo> => {
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
  data?: any
): Promise<T> => {
  // Setup
  const scheme = window.location.protocol; //"https";
  let baseURL = scheme + "//" + window.location.host + window.location.pathname;
  //console.log("base url = " + baseURL);
  //console.log("root url = " + process.env.ROOT_URL);

  if (process.env.ROOT_URL) {
    //console.log("dev root url is not empty");
    baseURL = process.env.ROOT_URL;
  }

  if (!baseURL.endsWith("/")) {
    baseURL += "/";
  }

  const apiUrl = `${baseURL}api/${API_VERSION}/`;

  let cacheControl = `&_=${Date.now()}`; // don't cache stuff
  if (url.includes("html")) {
    cacheControl = "";
  }

  const urlToCall = `${apiUrl}${url}?v=${appVersion}${cacheControl}`;

  const fetchOptions: RequestInit = {
    method: method,
  };

  if (method == "POST") {
    fetchOptions.body = data;
  }

  // make API call
  try {
    const response = await fetch(urlToCall, fetchOptions);

    if (response.status >= 200 && response.status < 300) {
      // success
      var json = (await response.json()) as T;
      return Promise.resolve(json);
    } else if (response.status == 401) {
      //todo: Unauthorized:
      console.log("unauthorized");
    } else {
      alert("Unhandled response status: " + response.status);
      return null;
      //TODO: handle response
    }
  } catch (err) {
    console.log(err);

    //TODO: show these errors somewhere
    alert(
      "Error contacting the server. You might not have internet or our server is down"
    );
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
