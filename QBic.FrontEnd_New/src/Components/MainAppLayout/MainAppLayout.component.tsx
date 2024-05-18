import {
  AppLayout,
  AppLayoutProps,
  SideNavigation,
  SideNavigationProps,
  TopNavigation,
} from "@cloudscape-design/components";
import { useEffect, useState } from "react";
import { MenuItem, TestMenuData } from "../../TestData/Menus";
import { useLocation, useNavigate } from "react-router-dom";

interface MainAppLayoutProps extends AppLayoutProps {
  content: React.ReactNode;
}

interface AppMenuItem {
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

const findClickedItem = (id: string, items: AppMenuItem[]): AppMenuItem => {
  for (const item of items) {
    if (item.id === id) {
      return item;
    }
    if (item.subMenus) {
      const foundItem = findClickedItem(id, item.subMenus);
      if (foundItem) {
        return foundItem;
      }
    }
  }
  return null;
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

const rootMenuItem: AppMenuItem = {
  id: "root",
  canDelete: false,
  event: null,
  href: "/",
  name: "",
  parentMenu: null,
  path: "/",
  position: 0,
  subMenus: null,
};

const allItems = MapMenuItemToAppMenuItem(TestMenuData as MenuItem[], ""); //TODO: fetch this async etc.
rootMenuItem.subMenus = allItems;

export const MainAppLayout = ({ content }: MainAppLayoutProps) => {
  const [currentMenuItem, setCurrentMenuItem] =
    useState<AppMenuItem>(rootMenuItem);

  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    // TODO: can check location.pathname here to see if we are already on a path, and handle that as a click event or similar
    console.log(location.pathname);
    const fullPath = location.pathname.substring(1);
    console.log(fullPath);
  }, []);

  useEffect(() => {
    console.log("location changed");
    console.log(location);
    console.log(currentMenuItem);
  }, [location]);

  const handleMenuClick = (itemId: string) => {
    console.log("menu item clicked");
    console.log(itemId);

    if (!itemId || itemId == "/") {
      console.log("item id is null");
      navigate("/");
      return;
    }

    if (itemId.indexOf("#back") > -1) {
      console.log("back clicked");
      const parentId = itemId.replace("#back/", "");
      console.log(parentId);
      const parentMenu = findClickedItem(parentId, allItems);
      console.log("parent menu", parentMenu);
      setCurrentMenuItem(parentMenu.parentMenu);
      navigate(-1);
      return;
    }

    const menuItemClicked = findClickedItem(itemId, allItems);

    if (menuItemClicked) {
      navigate(menuItemClicked.href);
      if (menuItemClicked.subMenus && menuItemClicked.subMenus.length > 1) {
        // show sub-menu items
        console.log("setting current menuitem", menuItemClicked);
        console.log(currentMenuItem);
        menuItemClicked.parentMenu = currentMenuItem;
        setCurrentMenuItem(menuItemClicked);
      } else {
        // todo: process the event
        console.log("TODO: Process event: " + menuItemClicked.event);
      }
    }
  };

  return (
    <>
      <div id="h" style={{ position: "sticky", top: 0, zIndex: 1002 }}>
        <TopNavigation
          // i18nStrings={i18nStrings}
          identity={{
            href: "/",
            title: "App name",
            //   logo: { src: logo, alt: "Service name logo" },
          }}
          // search={
          //   <Input
          //     ariaLabel="Input field"
          //     clearAriaLabel="Clear"
          //     value={searchValue}
          //     type="search"
          //     placeholder="Search"
          //     onChange={({ detail }) => setSearchValue(detail.value)}
          //   />
          // }
          // utilities={[
          //   {
          //     type: "button",
          //     iconName: "notification",
          //     ariaLabel: "Notifications",
          //     badge: true,
          //     disableUtilityCollapse: true,
          //   },
          //   {
          //     type: "button",
          //     iconName: "settings",
          //     title: "Settings",
          //     ariaLabel: "Settings",
          //   },
          //   {
          //     type: "menu-dropdown",
          //     text: "Customer name",
          //     description: "customer@example.com",
          //     iconName: "user-profile",
          //     items: profileActions,
          //   },
          //]}
        />
      </div>
      <AppLayout
        headerSelector="#h"
        content={content}
        toolsHide={true}
        navigation={
          <SideNavigation
            activeHref={currentMenuItem.href}
            // header={{ href: currentMenuItem.href, text: currentMenuItem.name }}
            onFollow={(event) => {
              event.preventDefault();
              handleMenuClick(event.detail.href);
            }}
            items={currentMenuItem.subMenus.map(
              (x) =>
                ({
                  type: "link",
                  text: x.name,
                  href: x.href,
                } as SideNavigationProps.Link)
            )}
          />
        }
      ></AppLayout>{" "}
    </>
  );
};
