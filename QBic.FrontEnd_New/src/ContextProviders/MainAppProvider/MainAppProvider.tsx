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
  component: React.ReactNode;
  id: string;
  splitPanelContents: () => React.ReactNode;
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
  setSelectedTableRow: (selection: SelectedRecord) => void;
  selectedRow: SelectedRecord;
}

interface SelectedRecord {
  menuItem: MenuDetail;
  rowData: any;
}

const MainAppContext = createContext<MainAppContextType>(null);

const homeDisplayItem: DisplayItem = {
  menu: null,
  type: "home",
  visible: true,
  id: "home",
  component: <Home />,
  splitPanelContents: () => null,
};

let clearingStack = false;

export const MainAppProvider = ({ children }) => {
  const [appName, setAppName] = useState("");
  const [appVersion, setAppVersion] = useState("");
  const [dateFormat, setDateFormat] = useState("");
  const [isReady, setIsReady] = useState(false);

  const [selectedRow, setRow] = useState<SelectedRecord>(null);

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

  const getComponent = (display: ComponentInfo) => {
    const uid = uuidv4();
    console.log("get component called " + display?.menu?.Title + " -> " + uid);

    switch (display.type) {
      case "login":
        return <Login />;
      case "form":
        return <FormComponent menuItem={display.menu} xx={uid} />; //TODO: not creating a new copy of form - all state values need to be handled somewhere else then
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
      id: uuidv4(),
      visible: visible,
      menu: item.menu,
      type: item.type,
      component: getComponent(item),
      splitPanelContents: () => null, // can do this the same as getComponent and pass in the menu item, i think it will only render once
      // but how to link selected item? also via mainApp?
    };
    //TODO: if view, set split panel stuff somehow
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

    setDisplayStack(stack);
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

  const setSelectedTableRow = (selection: SelectedRecord): void => {
    // console.log
    setRow(selection);
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
    showComponent,
    displayStack,
    clearDisplayStack,
    setSelectedTableRow,
    selectedRow,
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
