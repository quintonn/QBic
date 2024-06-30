import { createContext, useContext, useEffect, useState } from "react";
import { API_URL } from "../../Constants/AppValues";
import { useMainApp } from "../MainAppProvider/MainAppProvider";
import { useNavigate } from "react-router-dom";

interface AuthContextType {
  refreshToken: string;
  getAccessToken: () => string;
  doLogin: (username: string, password: string) => void;
  logout: () => void;
  isAuthenticated: boolean;
  performTokenRefresh: () => Promise<void>;
  resetPassword: (username: string) => Promise<string>;
  user: UserInfo;
}

interface UserInfo {
  Id: string;
  User: string;
}

const AuthContext = createContext<AuthContextType>(null);

let accessToken = "";

export const AuthProvider = ({ children }) => {
  //const [accessToken, setAccessToken] = useState("");
  const [refreshToken, setRefreshToken] = useState("");
  const [lastRefreshDate, setLastRefreshDate] = useState<Date>(new Date());
  const [user, setUser] = useState<UserInfo>(null);

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
    // this is done using a useEffect, else the accessToken and refreshToken state values are not set
    // TODO: Can be improved
    if (gotTokens === true) {
      validateRefreshToken();
    }
  }, [gotTokens]);

  const getAccessToken = () => {
    return accessToken;
  };

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

        accessToken = json.access_token;
        setRefreshToken(json.refresh_token);

        setLastRefreshDate(new Date());

        localStorage.setItem(getName("accessToken"), json.access_token);
        localStorage.setItem(getName("refreshToken"), json.refresh_token);
        localStorage.setItem(
          getName("lastRefreshDate"),
          JSON.stringify(new Date())
        );

        setIsAuthenticated(true);
        return Promise.resolve();
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
      try {
        await performTokenRefresh();
      } catch (err) {
        console.log("error performing token refresh");
        console.log(err);
      }
      setIsReady(true);
      onReadyFunction();
    } else {
      setIsReady(true);
      onReadyFunction();
    }
  };

  const initializeAuth = async () => {
    // get auth tokens from local storage
    accessToken = localStorage.getItem(getName("accessToken"));
    const _refreshToken = localStorage.getItem(getName("refreshToken"));
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

  const resetPassword = async (username: string) => {
    const urlToCall = `${API_URL}menu/RequestPasswordReset?v=${appVersion}`;

    // make API call
    try {
      const fetchOptions: RequestInit = {
        method: "POST",
        body: JSON.stringify({ usernameOrEmail: username }),
      };

      const apiResponse = await fetch(urlToCall, fetchOptions);

      if (apiResponse.ok === true) {
        return await apiResponse.text();
      }

      const responseText = await apiResponse.text();

      return Promise.reject(responseText);
    } catch (err) {
      console.log(err);
    }
  };

  const doLogin = async (username: string, password: string) => {
    await performLogin(username, password);
  };

  const logout = () => {
    localStorage.removeItem(getName("accessToken"));
    localStorage.removeItem(getName("refreshToken"));
    localStorage.removeItem(getName("lastRefreshDate"));
    setIsAuthenticated(false);
    window.location.reload(); // TODO: instead of reloading the page, call all the initialization code again
  };

  async function onReadyFunction(allow401: boolean = true) {
    // call initialize (basically checks if user is authenticated, and returns user name and id)

    // make API call
    try {
      const fetchOptions: RequestInit = {
        method: "GET",
        headers: {
          Authorization: "Bearer " + accessToken,
        },
      };

      const urlToCall = `${API_URL}initialize?v=${appVersion}`;
      const apiResponse = await fetch(urlToCall, fetchOptions);

      if (apiResponse.ok === true) {
        const json = await apiResponse.json();
        const userInfo = json as UserInfo;

        setUser(userInfo);
        setIsAuthenticated(true);
      } else {
        if (apiResponse.status == 401) {
          if (allow401) {
            console.log("got a 401");
            await performTokenRefresh();
            console.log("after perform refresh token");
            await onReadyFunction(false);
          } else {
            navigate("/login");
          }
        }
      }
      //return Promise.reject(responseText);
    } catch (err) {
      console.log("caught error trying to initialize");
      console.log(err);
      navigate("/login"); // not sure what else to do
    }
  }

  const value = {
    refreshToken,
    getAccessToken,
    performTokenRefresh,
    isReady, //TODO: i would like this to work without all this isReady stuff
    doLogin,
    logout,
    isAuthenticated,
    setIsAuthenticated,
    resetPassword,
    user,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
