import { createContext, useContext, useEffect, useState } from "react";
import { useAppInfo } from "../AppInfoContextProvider/AppInfoContextProvider";

export const AuthContext = createContext(null);

export const AuthContextProvider = ({ children }) => {
  const [accessToken, setAccessToken] = useState("");
  const [refreshToken, setRefreshToken] = useState("");
  const [lastRefreshDate, setLastRefreshDate] = useState<Date>(new Date());
  const appInfo = useAppInfo();

  const [isReady, setIsReady] = useState(false);

  const getName = (name: string) => {
    return appInfo.appName + "_" + name;
  };

  const performTokenRefresh = async () => {
    const data = new FormData();
    data.append("grant_type", "refresh_token");
    data.append("refresh_token", refreshToken);
    data.append("client_id", appInfo.appName);

    const urlToCall = `${appInfo.apiUrl}token?v=${appInfo.appVersion}`;

    // make API call
    try {
      const fetchOptions: RequestInit = {
        method: "POST",
      };

      fetchOptions.body = data;

      const loginResponse = await fetch(urlToCall, fetchOptions);

      console.log(loginResponse);

      if (loginResponse.ok === true) {
        // TODO: update token stuff
        console.log("TODO: update auth tokens");
      }
      console.log("login response not ok");
      return Promise.reject("could not update refresh token");
    } catch (err) {
      console.log(err);
    }
  };

  const validateRefreshToken = async () => {
    // check when last the refresh token was obtained
    const refreshDate = localStorage.getItem(getName("lastRefreshDate"));
    let lastRefreshDateString = refreshDate || "";
    if (lastRefreshDateString == null || lastRefreshDateString.length == 0) {
      // this defaults it to right now, so it won't try and refresh the token
      // The next API call will fail with 401 and force the login dialog
      lastRefreshDateString = JSON.stringify(new Date());
    }
    const savedLastRefreshDate = new Date(JSON.parse(lastRefreshDateString));
    setLastRefreshDate(savedLastRefreshDate);

    // see if refresh token needs to be refreshed again
    var today = new Date();

    if (
      savedLastRefreshDate.setHours(0, 0, 0, 0) < today.setHours(0, 0, 0, 0)
    ) {
      console.log("last refresh was before today");
      await performTokenRefresh();
      console.log("after perform token refresh");
      setIsReady(true);
    } else {
      console.log("not refreshing token, setting ready");
      setIsReady(true);
    }
  };

  const initializeAuth = async () => {
    // get auth tokens from local storage

    const accessToken = localStorage.getItem(getName("accessToken"));
    const refreshToken = localStorage.getItem(getName("refreshToken"));
    setAccessToken(accessToken);
    setRefreshToken(refreshToken);

    console.log("initializing auth");
    console.log(accessToken, refreshToken, "tokens <---");

    //setIsReady(true);
    validateRefreshToken();
  };

  const performLogin = async (username: string, password: string) => {
    const data = new FormData();
    data.append("grant_type", "password");
    data.append("username", username);
    data.append("password", password);
    data.append("client_id", appInfo.appName);

    const urlToCall = `${appInfo.apiUrl}token?v=${appInfo.appVersion}`;

    // make API call
    try {
      const fetchOptions: RequestInit = {
        method: "POST",
      };

      fetchOptions.body = data;

      const loginResponse = await fetch(urlToCall, fetchOptions);

      console.log(loginResponse);

      if (loginResponse.ok === true) {
        // TODO: update token stuff
        console.log("TODO: update auth tokens");
        const json = await loginResponse.json();
        console.log(json);

        //setAccessToken(json.access_token);
        //setRefreshToken(json.refresh_token);
        //setLastRefreshDate(new Date());

        localStorage.setItem(getName("accessToken"), json.access_token);
        localStorage.setItem(getName("refreshToken"), json.refresh_token);
        localStorage.setItem(
          getName("lastRefreshDate"),
          JSON.stringify(new Date())
        );

        console.log("reloading page");
        window.location.reload();
        return;
      }
      console.log("login response not ok");
      return Promise.reject("could not login");
    } catch (err) {
      console.log(err);
    }
  };

  const doLogin = async () => {
    //return;
    console.log("xx loging in XX");

    // THIS WORKED - make UI for this
    // ALSO - need a log out button

    // let username = "admin";
    // let password = "password";

    // try {
    //   await performLogin(username, password);
    // } catch (err) {
    //   console.log("error logging in");
    //   console.log(err);
    // }
  };

  const value = {
    refreshToken,
    accessToken,
    initializeAuth,
    performTokenRefresh,
    isReady,
    doLogin,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthContextProvider");
  }
  return context;
};
