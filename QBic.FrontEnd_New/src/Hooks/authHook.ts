import { useEffect, useState } from "react";
import { useMainApp } from "../ContextProviders/MainAppProvider/MainAppProvider";
import { API_URL } from "../Constants/AppValues";
import { useNavigate } from "react-router-dom";

export const useAuth = () => {
  const [accessToken, setAccessToken] = useState("");
  const [refreshToken, setRefreshToken] = useState("");
  const [lastRefreshDate, setLastRefreshDate] = useState<Date>(new Date());

  const [isReady, setIsReady] = useState(false);

  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [gotTokens, setGotTokens] = useState(false);

  const { appName, appVersion, isReady: mainAppIsReady } = useMainApp();

  const navigate = useNavigate();

  const getName = (name: string) => {
    return appName + "_" + name;
  };

  useEffect(() => {
    if (mainAppIsReady === true) {
      initializeAuth();
    }
  }, [mainAppIsReady]);

  useEffect(() => {
    if (gotTokens === true) {
      validateRefreshToken();
    }
  }, [gotTokens]);

  const performTokenRefresh = async () => {
    const data = new FormData();
    data.append("grant_type", "refresh_token");
    data.append("refresh_token", refreshToken);
    data.append("client_id", appName);

    const urlToCall = `${API_URL}token?v=${appVersion}`;

    // make API call
    try {
      const fetchOptions: RequestInit = {
        method: "POST",
      };

      fetchOptions.body = data;

      const loginResponse = await fetch(urlToCall, fetchOptions);

      if (loginResponse.ok === true) {
        const json = await loginResponse.json();

        setAccessToken(json.access_token);
        setRefreshToken(json.refresh_token);

        setLastRefreshDate(new Date());

        localStorage.setItem(getName("accessToken"), json.access_token);
        localStorage.setItem(getName("refreshToken"), json.refresh_token);
        localStorage.setItem(
          getName("lastRefreshDate"),
          JSON.stringify(new Date())
        );

        setIsAuthenticated(true);
      }
      return Promise.reject("could not update refresh token");
    } catch (err) {
      console.error(err);
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
      await performTokenRefresh();
      setIsReady(true);
    } else {
      setIsReady(true);
    }
  };

  const initializeAuth = async () => {
    // get auth tokens from local storage
    const _accessToken = localStorage.getItem(getName("accessToken"));
    const _refreshToken = localStorage.getItem(getName("refreshToken"));
    setAccessToken(_accessToken);
    setRefreshToken(_refreshToken);

    setGotTokens(true);
  };

  const performLogin = async (username: string, password: string) => {
    const data = new FormData();
    data.append("grant_type", "password");
    data.append("username", username);
    data.append("password", password);
    data.append("client_id", appName);

    const urlToCall = `${API_URL}token?v=${appVersion}`;

    // make API call
    try {
      const fetchOptions: RequestInit = {
        method: "POST",
      };

      fetchOptions.body = data;

      const loginResponse = await fetch(urlToCall, fetchOptions);

      if (loginResponse.ok === true) {
        const json = await loginResponse.json();

        localStorage.setItem(getName("accessToken"), json.access_token);
        localStorage.setItem(getName("refreshToken"), json.refresh_token);
        localStorage.setItem(
          getName("lastRefreshDate"),
          JSON.stringify(new Date())
        );

        setTimeout(() => {
          window.location.reload();
        }, 10); // make sure this happens after navigate !?!
        navigate("/");

        return;
      }

      const responseText = await loginResponse.text();

      return Promise.reject(responseText);
    } catch (err) {
      console.log(err);
    }
  };

  const doLogin = async (username: string, password: string) => {
    await performLogin(username, password);
  };

  const logout = () => {
    console.log("auth logout called");
    localStorage.removeItem(getName("accessToken"));
    localStorage.removeItem(getName("refreshToken"));
    localStorage.removeItem(getName("lastRefreshDate"));
    setIsAuthenticated(false);
    window.location.reload(); // TODO: instead of reloading the page, call all the initialization code again
  };

  return {
    refreshToken,
    accessToken,
    initializeAuth,
    performTokenRefresh,
    isReady, //TODO: i would like this to work without all this isReady stuff
    doLogin,
    logout,
    isAuthenticated,
    setIsAuthenticated,
  };
};
