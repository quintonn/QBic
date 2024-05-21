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
    console.log("performing token refresh");
    console.log(appInfo);

    const data = new FormData();
    data.append("grant_type", "refresh_token");
    data.append("refresh_token", refreshToken);
    data.append("client_id", appInfo.appName);

    console.log(appInfo);
    const urlToCall = `${appInfo.apiUrl}token?v=${appInfo.appVersion}`;

    // make API call
    try {
      const fetchOptions: RequestInit = {
        method: "POST",
      };

      fetchOptions.body = data;

      const loginResponse = await fetch(urlToCall, fetchOptions);

      console.log(loginResponse);
      if (loginResponse) {
        // TODO: update token stuff
        console.log("TODO: update auth tokens");
      }
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
    //console.log('today: ', today);
    if (
      savedLastRefreshDate.setHours(0, 0, 0, 0) < today.setHours(0, 0, 0, 0)
    ) {
      console.log("last refresh was before today");
      await performTokenRefresh();
    } else {
      console.log("not refreshing token");
    }
  };

  const initializeAuth = async () => {
    // get auth tokens from local storage
    console.log("init auth");
    console.log(appInfo);
    const accessToken = localStorage.getItem(getName("accessToken"));
    const refreshToken = localStorage.getItem(getName("refreshToken"));
    setAccessToken(accessToken);
    setRefreshToken(refreshToken);

    console.log("auth initialized, setting is ready");
    setIsReady(true);
  };

  useEffect(() => {
    if (isReady === true) {
      validateRefreshToken();
    }
  }, [isReady]);
  const value = { refreshToken, initializeAuth, performTokenRefresh, isReady };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthContextProvider");
  }
  return context;
};
