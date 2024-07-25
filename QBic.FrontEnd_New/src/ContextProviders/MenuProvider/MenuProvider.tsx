import { createContext, useContext, useEffect, useState } from "react";
import { SideNavigationProps } from "@cloudscape-design/components";
import { useAuth } from "../AuthProvider/AuthProvider";
import { useApi } from "../../Hooks/apiHook";
import { useMainApp } from "../MainAppProvider/MainAppProvider";
import { useActions } from "../ActionProvider/ActionProvider";
import { store } from "../../App/store";
import { addMessage } from "../../App/flashbarSlice";

interface MenuContextType {
  appMenuItems: AppMenuItem[];
  sideNavMenuItems: SideNavigationProps.Item[];
  onHomeClick: () => Promise<void>;
}

const MenuContext = createContext<MenuContextType>(null);

export interface MenuItem {
  Name: string;
  ParentMenu: null;
  SubMenus: MenuItem[] | null;
  Event: number | null;
  Position: number;
  Id: string;
  CanDelete: boolean;
}

export interface AppMenuItem {
  href: string;
  path: string;
  name: string;
  parentMenu: any;
  subMenus: AppMenuItem[] | null;
  event: number | null;
  position: number;
  id: string;
  canDelete: boolean;
}

export enum ColumnType {
  String = 0,
  Boolean = 1,
  Button = 2,
  Link = 3,
  Hidden = 4,
  Date = 5,
  CheckBox = 6,
}

enum ColumnDisplay {
  Show = 0,
  Hide = 1,
}

enum ConditionComparison {
  Equals = 0,
  NotEquals = 1,
  Contains = 2,
  IsNotNull = 3,
  IsNull = 4,
  GreaterThan = 5,
  GreaterThanOrEqual = 6,
  LessThan = 7,
  LessThanOrEqual = 8,
}
interface ColumnCondition {
  ColumnName: string;
  Comparison: ConditionComparison;
  ColumnValue: string;
}

interface ColumnSetting {
  ColumnSettingType: number;
  Display?: ColumnDisplay;
  Conditions?: ColumnCondition[];
}

export interface ViewEvent {
  ActionType: number;
  EventNumber: number;
  CancelButtonText?: string;
  ConfirmationButtonText?: string;
  ConfirmationMessage?: string;
  OnCancelUIAction?: number;
  OnConfirmationUIAction?: number;
}

export interface ViewColumn {
  ColumnSpan: number;
  ColumnLabel: string;
  ColumnName: string;
  ColumnType: ColumnType;
  ColumnSetting: ColumnSetting;
  KeyColumn?: string;
  EventNumber?: number;
  Event?: ViewEvent;
  LinkLabel?: string;
  TrueValueDisplay?: string;
  FalseValueDisplay?: string;
  ParametersToPass?: any;
}

export interface ListSourceItem {
  Key: string;
  Value: string;
}

export interface VisibilityConditions {
  ColumnName: string;
  ColumnValue: string;
  Comparison: number;
}

export interface InputField {
  DefaultValue: any;
  InputLabel: string;
  InputName: string;
  InputType: number;
  Mandatory: boolean;
  MandatoryConditions: any[];
  RaisePropertyChangedEvent: boolean;
  TabName: string | null;
  VisibilityConditions: VisibilityConditions[];
  MultiLineText?: boolean;
  ListItems?: ListSourceItem[];
  ListSource?: ListSourceItem[];
  ViewForInput?: MenuDetail;
}

export interface InputButton {
  ActionNumber: number;
  Label: string;
  ValidateInput: boolean;
}

export interface MenuDetail {
  Description?: string;
  Title?: string;
  AllowInMenu?: boolean;
  DataForGettingMenu?: any;
  ActionType?: number;
  Pages?: 0;
  LinesPerPage?: number;
  CurrentPage?: number;
  TotalLines?: number;
  Filter?: string;
  RequiresAuthorization?: boolean;
  EventParameters?: any;
  Id?: number;
  Parameters?: any;
  Columns?: ViewColumn[];
  ViewData?: any[];
  ParametersToPass?: any;
  EventNumber?: number;
  AllowSorting?: boolean;
  DataUrl?: string;
  RequestData?: string;
  InputFields?: InputField[];
  InputButtons?: InputButton[];
  CancelButtonText?: string;
  ConfirmationButtonText?: string;
  ConfirmationMessage?: string;
  OnCancelUIAction?: number;
  OnConfirmationUIAction?: number;
  InputName?: string;
  InputIsVisible?: boolean;
  ListItems?: ListSourceItem[];
  JsonDataToUpdate?: string;
  UpdateType?: number;
}

const MapMenuItemsToSideNavItems = (
  items: MenuItem[],
  root: boolean = true
): SideNavigationProps.Item[] => {
  const results: SideNavigationProps.Item[] = [];

  if (root) {
    results.push({ text: "Home", href: "/", type: "link" });
  }

  for (let i = 0; i < items?.length; i++) {
    const item = items[i];
    if (item.SubMenus && item.SubMenus.length > 0) {
      const sectionItem = {
        text: item.Name,
        type: "section",
        defaultExpanded: false,
        items: MapMenuItemsToSideNavItems(item.SubMenus, false),
      } as SideNavigationProps.Item;

      results.push(sectionItem);
    } else {
      //href: "#/view" + item.Event, // TODO: Make open in new tab on menu also work
      results.push({ text: item.Name, href: "/" + item.Id, type: "link" });
    }
  }

  return results;
};

const MapMenuItemToAppMenuItem = (
  items: MenuItem[],
  path: string = ""
): AppMenuItem[] => {
  if (!items) {
    return [];
  }
  return items.map((item) => ({
    path: path + "/" + item.Id,
    href: item.Id,
    id: item.Id,
    name: item.Name,
    parentMenu: null,
    subMenus: [
      {
        id: "#back",
        event: null,
        position: 0,
        canDelete: false,
        href: "#back/" + item.Id,
        name: "<<",
        parentMenu: null,
        path: "",
        subMenus: [],
      },
      ...MapMenuItemToAppMenuItem(item.SubMenus, path + "/" + item.Id),
    ],
    event: item.Event,
    position: item.Position,
    canDelete: item.CanDelete,
  }));
};

export const MenuProvider = ({ children }) => {
  const [appMenuItems, setAppMenuItems] = useState<AppMenuItem[]>([]);
  const [sideNavMenuItems, setSideNavMenuItems] = useState<
    SideNavigationProps.Item[]
  >([]);

  const auth = useAuth();
  const api = useApi();
  const { onMenuClick } = useActions();
  const {
    setCurrentContentType,
    isReady: mainAppIsReady,
    showComponent,
  } = useMainApp();

  const loadMenus = async () => {
    const menuData = await api.makeApiCall<MenuItem[]>("getUserMenu", "GET");
    const menuItems = MapMenuItemToAppMenuItem(menuData, "");

    const sideNavItems = MapMenuItemsToSideNavItems(menuData);

    setAppMenuItems(menuItems);
    setSideNavMenuItems(sideNavItems);
  };

  const onHomeClick = async () => {
    showComponent({ menu: null, type: "home" });
    setCurrentContentType("default");
  };

  useEffect(() => {
    if (auth.isAuthenticated === true) {
      loadMenus();
    } else {
      setAppMenuItems([]);
      setSideNavMenuItems([]);
    }
  }, [auth.isAuthenticated]);

  const checkPredefinedActions = async () => {
    if (window.location.search.includes("anonAction")) {
      const url = new URL(window.location.href);
      const action = url.searchParams.get("anonAction");
      const params = url.searchParams.get("params");

      await onMenuClick(parseInt(action), params);

      return;
    }

    if (window.location.search.includes("confirmed")) {
      store.dispatch(
        addMessage({
          type: "success",
          content: `"Thank you for confirming you email address. You can now log in using your username and password"`,
        })
      );

      const url = new URL(window.location.href);

      url.searchParams.delete("confirmed");
      window.history.pushState(null, "", url.toString());
    }
  };

  useEffect(() => {
    checkPredefinedActions();
  }, [mainAppIsReady]);

  const value = {
    appMenuItems,
    sideNavMenuItems,
    onHomeClick,
  };

  return <MenuContext.Provider value={value}>{children}</MenuContext.Provider>;
};

export const useMenu = () => {
  const context = useContext(MenuContext);
  if (!context) {
    throw new Error("useMenu must be used within an MenuProvider");
  }
  return context;
};
