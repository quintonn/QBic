import { createContext, useContext, useEffect, useState } from "react";
import { API_URL } from "../../Constants/AppValues";
import { AppLayoutProps } from "@cloudscape-design/components";
import { addMessage } from "../../App/flashbarSlice";
import { store } from "../../App/store";

import { v4 as uuidv4 } from "uuid";
import { MenuDetail } from "../MenuProvider/MenuProvider";
import { Login } from "../../Components/Login/Login.component";
import { FormComponent } from "../../Components/Form/Form.component";
import { ViewComponent } from "../../Components/View/View.component";
import { Home } from "../../Components/Home/Home.component";

export interface SystemInfo {
  ApplicationName: string;
  Version: string;
  ConstructionError: string;
  DateFormat: string;
}

type displayType = "form" | "view" | "home" | "login";

export interface DisplayItem extends ComponentInfo {
  visible: boolean;
  component: () => React.ReactNode;
  id: string;
}

export interface ComponentInfo {
  menu?: MenuDetail;
  type: displayType;
}

interface MainAppContextType {
  appName: string;
  appVersion: string;
  dateFormat: string;
  isReady: boolean;
  currentContentType: AppLayoutProps.ContentType;
  setCurrentContentType: (value: AppLayoutProps.ContentType) => void;
  inputViewUpdateData: any;
  setInputViewUpdateData: (data: any) => void;
  showComponent: (value: ComponentInfo | -1) => void;
  displayStack: DisplayItem[];
  clearDisplayStack: () => void;
}

const MainAppContext = createContext<MainAppContextType>(null);

const homeDisplayItem: DisplayItem = {
  menu: null,
  type: "home",
  visible: true,
  id: "home",
  component: () => <Home />,
};

let clearingStack = false;

export const MainAppProvider = ({ children }) => {
  const [appName, setAppName] = useState("");
  const [appVersion, setAppVersion] = useState("");
  const [dateFormat, setDateFormat] = useState("");
  const [isReady, setIsReady] = useState(false);

  const [currentContentType, setCurrentContentType] =
    useState<AppLayoutProps.ContentType>("default");

  const [displayStack, setDisplayStack] = useState<DisplayItem[]>([
    homeDisplayItem,
  ]);

  const makeLastItemVisible = (stack: DisplayItem[]) => {
    if (stack.length > 0) {
      stack[stack.length - 1].visible = true;
    }
  };

  const hideAllItems = (stack: DisplayItem[]) => {
    for (let i = 0; i < stack.length; i++) {
      stack[i].visible = false;
    }
  };

  const getComponent = (display: DisplayItem) => {
    switch (display.type) {
      case "login":
        return <Login />;
      case "form":
        return (
          <FormComponent menuItem={display.menu} visible={display.visible} />
        );
      case "view":
        return <ViewComponent menuItem={display.menu} />;
      case "home":
        return <Home />;
    }
  };

  const addDisplayItem = (
    item: ComponentInfo,
    visible: boolean,
    stack: DisplayItem[]
  ) => {
    const stackItem: DisplayItem = {
      component: () => null,
      id: uuidv4(),
      visible: visible,
      menu: item.menu,
      type: item.type,
    };
    stackItem.component = () => {
      return getComponent(stackItem);
    };
    stack.push(stackItem);
  };

  const showComponent = (displayItem: ComponentInfo | -1) => {
    let stack = [...displayStack];
    hideAllItems(stack);

    if (clearingStack === true) {
      clearingStack = false;
      stack = [homeDisplayItem];
    }

    if (displayItem == -1) {
      // remove current item
      if (stack.length > 0) {
        const index = stack.length - 1;
        const lastItem = stack[index];
        stack = [...stack.slice(0, index), ...stack.slice(index + 1)];
      }
      if (displayStack.length == 0) {
        addDisplayItem(homeDisplayItem, true, stack);
      }
    } else {
      addDisplayItem(displayItem, true, stack);
    }

    makeLastItemVisible(stack);

    setDisplayStack(stack); // TODO: this seems to work, but make all other items not visible and clean up code
  };

  const [inputViewUpdateData, setInputViewUpdateData] = useState<any>(null);

  const initializeSystem = async (): Promise<void> => {
    try {
      const cacheControl = `&_=${Date.now()}`; // don't cache stuff
      const urlToCall = `${API_URL}initializeSystem?v=${cacheControl}`;
      const resp = await fetch(urlToCall);
      const systemInfo = (await resp.json()) as SystemInfo;
      if (systemInfo) {
        if (systemInfo.ConstructionError) {
          console.log("There was an error in the system initialization code:");
          console.log(systemInfo.ConstructionError);

          store.dispatch(
            addMessage({
              type: "error",
              content: `"There was an error in the system initialization code:" ${systemInfo.ConstructionError}`,
            })
          );
        } else {
          setAppName(systemInfo.ApplicationName);
          setAppVersion(systemInfo.Version);
          setDateFormat(systemInfo.DateFormat);

          document.title = `${systemInfo.ApplicationName} ${systemInfo.Version}`;
          setIsReady(true);
        }
      }
    } catch (err) {
      console.error("Fatal error calling initialize system:");
      // wouldn't really happen because back-end hosts front-end
      console.error(err);
      store.dispatch(
        addMessage({
          type: "error",
          content: `Fatal error: ${err}`,
        })
      );
    }
  };

  useEffect(() => {
    initializeSystem();
  }, []);

  const clearDisplayStack = () => {
    clearingStack = true;
    const stack = [];
    addDisplayItem(homeDisplayItem, true, stack);
    setDisplayStack(stack);
  };

  const value = {
    appName,
    appVersion,
    dateFormat,
    isReady,
    currentContentType,
    setCurrentContentType,
    inputViewUpdateData,
    setInputViewUpdateData,
    //currentItem,
    showComponent,
    displayStack,
    clearDisplayStack,
  };

  return (
    <MainAppContext.Provider value={value}>{children}</MainAppContext.Provider>
  );
};

export const useMainApp = () => {
  const context = useContext(MainAppContext);
  if (!context) {
    throw new Error("useMainApp must be used within an MainAppProvider");
  }
  return context;
};
