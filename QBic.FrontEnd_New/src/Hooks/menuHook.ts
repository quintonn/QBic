import { useEffect, useState } from "react";
import {
  AppLayoutProps,
  SideNavigationProps,
} from "@cloudscape-design/components";
import { useAuth } from "../ContextProviders/AuthProvider/AuthProvider";
import { useApi } from "./apiHook";
import { useNavigate } from "react-router-dom";

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

interface MenuDetail {
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
}

const MapMenuItemsToSideNavItems = (
  items: MenuItem[],
  root: boolean = true
): SideNavigationProps.Item[] => {
  const results: SideNavigationProps.Item[] = [];

  if (root) {
    results.push({ text: "Home", href: "#", type: "link" });
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
      results.push({ text: item.Name, href: "#" + item.Id, type: "link" });
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

export const useMenus = () => {
  const [appMenuItems, setAppMenuItems] = useState<AppMenuItem[]>([]);
  const [sideNavMenuItems, setSideNavMenuItems] = useState<
    SideNavigationProps.Item[]
  >([]);

  const [menuCache, setMenuCache] = useState<Record<number, MenuDetail[]>>({}); // Maybe i shouldn't cache? Some stuff should reload
  const [currentContentType, setCurrentContentType] =
    useState<AppLayoutProps.ContentType>("default");

  const [currentMenu, setCurrentMenu] = useState<MenuDetail>({
    Description: "test",
  });

  const auth = useAuth();
  const api = useApi();
  const navigate = useNavigate();

  const loadMenus = async () => {
    const menuData = await api.makeApiCall<MenuItem[]>("getUserMenu", "GET");
    const menuItems = MapMenuItemToAppMenuItem(menuData, "");

    console.log(menuData);

    const sideNavItems = MapMenuItemsToSideNavItems(menuData);

    setAppMenuItems(menuItems);
    setSideNavMenuItems(sideNavItems);

    //TODO: check current Path and perform the on click so the page is updated
  };

  const onHomeClick = async () => {
    navigate("/");
    setCurrentContentType("default");
  };

  const onMenuClick = async (event: number) => {
    console.log("on menu click", event);

    let menuDetails = menuCache[event];

    if (!menuDetails) {
      const url = "executeUIAction/" + event;

      menuDetails = await api.makeApiCall<MenuDetail[]>(url, "POST");

      const newCache = { ...menuCache };
      newCache[event] = menuDetails;
      setMenuCache(newCache);
    }

    for (let i = 0; i < menuDetails.length; i++) {
      const item = menuDetails[i];

      switch (item.ActionType) {
        case 0: {
          console.log("setting current menu");
          console.log(item);
          console.log(item as MenuDetail);

          setCurrentContentType("table");
          setCurrentMenu({ Description: "abc" });
          navigate("/view/" + event);
          setCurrentMenu({ Description: "abcxxx" });

          break;
        }
        default:
          console.warn("Unknown action type: " + item.ActionType);
        // show global message?
      }

      break; // don't perform multiple actions for now, need to figure out how this will work
    }
  };

  useEffect(() => {
    if (auth.isAuthenticated === true) {
      loadMenus();
    } else {
      setAppMenuItems([]);
      setSideNavMenuItems([]);
    }
  }, [auth.isAuthenticated]);

  return {
    appMenuItems,
    sideNavMenuItems,
    onMenuClick,
    currentContentType,
    onHomeClick,
    currentMenu,
  };
};
