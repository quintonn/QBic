import { useEffect, useState } from "react";
import { useMainApp } from "../ContextProviders/MainAppProvider/MainAppProvider";

export const useAuth = () => {
  const [accessToken, setAccessToken] = useState("");
  const [refreshToken, setRefreshToken] = useState("");
  const [lastRefreshDate, setLastRefreshDate] = useState<Date>(new Date());

  const [isReady, setIsReady] = useState(false);

  const [isAuthenticated, setIsAuthenticated] = useState(false);

  const {
    appName,
    apiUrl,
    appVersion,
    baseUrl,
    isReady: mainAppIsReady,
  } = useMainApp();

  const getName = (name: string) => {
    return appName + "_" + name;
  };

  useEffect(() => {
    if (mainAppIsReady === true) {
      console.log("auth hook got main app ready");
      initializeAuth();
    }
  }, [mainAppIsReady]);

  const performTokenRefresh = async () => {
    const data = new FormData();
    data.append("grant_type", "refresh_token");
    data.append("refresh_token", refreshToken);
    data.append("client_id", appName);

    const urlToCall = `${apiUrl}token?v=${appVersion}`;

    // make API call
    try {
      const fetchOptions: RequestInit = {
        method: "POST",
      };

      fetchOptions.body = data;

      const loginResponse = await fetch(urlToCall, fetchOptions);

      console.log(loginResponse);

      if (loginResponse.ok === true) {
        const json = await loginResponse.json();
        console.log(json);

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
      console.log("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx");
      console.log(refreshToken);
      console.log(savedLastRefreshDate);
      console.log(today);
      await performTokenRefresh();
      console.log("after perform token refresh");
      //setIsReady(true);
    } else {
      console.log("not refreshing token, setting ready");
      //setIsReady(true);
    }
  };

  const initializeAuth = async () => {
    // get auth tokens from local storage
    const _accessToken = localStorage.getItem(getName("accessToken"));
    const _refreshToken = localStorage.getItem(getName("refreshToken"));
    setAccessToken(_accessToken);
    setRefreshToken(_refreshToken);

    console.log("initializing auth");
    console.log(accessToken);
    console.log(refreshToken);
    console.log("tokens <---");

    //setIsReady(true);
    try {
      await validateRefreshToken();
    } catch (err) {
      console.log("error validating auth tokens", err);
    } finally {
      const urlToCall = `${apiUrl}initialize?v=${appVersion}`;

      // make API call
      try {
        const fetchOptions: RequestInit = {
          method: "GET",
          headers: {
            Authorization: "Bearer " + accessToken,
          },
        };
        const userResponse = await fetch(urlToCall, fetchOptions);
        if (userResponse.ok) {
          const userInfo = await userResponse.json();
          setIsAuthenticated(true);
        } else {
          console.log("initialize call failed " + userResponse.status);
          setIsAuthenticated(false);
        }
      } catch (err) {
        setIsAuthenticated(false);
        console.log("initialize call failed");
        console.log(err);
        //TODO: so it means we're unauthenticated
      }

      setIsReady(true);
    }
  };

  const performLogin = async (username: string, password: string) => {
    const data = new FormData();
    data.append("grant_type", "password");
    data.append("username", username);
    data.append("password", password);
    data.append("client_id", appName);

    const urlToCall = `${apiUrl}token?v=${appVersion}`;

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

    // HERE ----> x123  -> Try implement the react-redux (see flashbar in envoy tools app)
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

  const logout = () => {
    // localStorage.setItem(getName("accessToken"), '');
    // localStorage.setItem(getName("refreshToken"), '');
    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("lastRefreshDate");

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
  };
};
