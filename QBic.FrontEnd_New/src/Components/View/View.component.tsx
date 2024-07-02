import { useEffect, useState } from "react";
import { MenuDetail } from "../../ContextProviders/MenuProvider/MenuProvider";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";

import { useLocation } from "react-router-dom";
import { TableComponent } from "./Table.component";

export const ViewComponent = () => {
  const location = useLocation();

  const [currentMenu, setCurrentMenu] = useState<MenuDetail>();
  const mainApp = useMainApp();

  useEffect(() => {
    if (location && location.pathname) {
      const menuItem = mainApp.getCacheValue(location.pathname);
      setCurrentMenu(menuItem);
    }
  }, [location]);

  return <TableComponent menuItem={currentMenu}></TableComponent>;
};
