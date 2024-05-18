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
  parentMenu: null;
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
    subMenus: MapMenuItemToAppMenuItem(item.SubMenus, path + "/" + item.Id),
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

const initialItems = allItems;

export const MainAppLayout = ({ content }: MainAppLayoutProps) => {
  const [items, setItems] = useState<AppMenuItem[]>(initialItems);

  const [currentMenuItem, setCurrentMenuItem] =
    useState<AppMenuItem>(rootMenuItem);

  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    // TODO: can check location.pathname here to see if we are already on a path, and handle that as a click event or similar
  }, []);

  const handleMenuClick = (itemId: string) => {
    console.log("menu item clicked");
    const menuItemClicked = findClickedItem(itemId, allItems);

    console.log(menuItemClicked);
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
            header={{ href: currentMenuItem.href, text: currentMenuItem.name }} // This can be updated with the current menu list ??
            onFollow={(event) => {
              event.preventDefault();
              handleMenuClick(event.detail.href);
            }}
            items={items.map(
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
