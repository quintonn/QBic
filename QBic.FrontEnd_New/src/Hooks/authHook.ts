import { useState } from "react";
import { useAppInfo } from "./appInfoHook";
import { makeApiCall } from "../Services/apiService";

export const useAuth = () => {
  const [accessToken, setAccessToken] = useState("");
  const [refreshToken, setRefreshToken] = useState("");
  const [lastRefreshDate, setLastRefreshDate] = useState<Date>(new Date());
  const appInfo = useAppInfo();

  const getName = (name: string) => {
    return appInfo.appName + "_" + name;
  };

  const performTokenRefresh = async (refreshToken: string, appName: string) => {
    console.log("performing token refresh");

    const data = {
      grant_type: "refresh_token",
      refresh_token: refreshToken,
      client_id: appName,
    };

    const loginResponse = await makeApiCall<any>("token", "POST", data, false);
    console.log(loginResponse);
    if (loginResponse) {
      // TODO: update token stuff
      console.log("TODO: update auth tokens");
    }
  };

  const validateRefreshToken = async (refreshToken) => {
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
      await performTokenRefresh(refreshToken, appInfo.appName);
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

    await validateRefreshToken(refreshToken);
  };

  return { initializeAuth, performTokenRefresh };
};
