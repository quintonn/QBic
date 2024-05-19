import { useState } from "react";

export const useAppInfo = () => {
  const [appName, setAppName] = useState("");
  const [appVersion, setAppVersion] = useState("");

  return { appName, setAppName, appVersion, setAppVersion };
};
