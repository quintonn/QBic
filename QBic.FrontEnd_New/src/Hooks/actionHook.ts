import { useNavigate } from "react-router-dom";
import {
  MenuDetail,
  ViewEvent,
} from "../ContextProviders/MenuProvider/MenuProvider";
import { useModal } from "../ContextProviders/ModalProvider/ModalProvider";
import { useApi } from "./apiHook";
import { useMainApp } from "../ContextProviders/MainAppProvider/MainAppProvider";

export const useActions = () => {
  const navigate = useNavigate();
  const modal = useModal();
  const api = useApi();
  const mainApp = useMainApp();

  const handleAction = (item: MenuDetail) => {
    switch (item.ActionType) {
      case 0: {
        // show a view
        mainApp.setCurrentContentType("table");
        //setCurrentMenu(item);
        mainApp.setCacheValue("/view/" + item.Id, item);
        navigate("/view/" + item.Id);
        break;
      }
      case 5: {
        // ShowMessage

        modal.getUserConfirmation(item as ViewEvent, null);
        // console.log("get user confirmation");
        // console.log(item);
        break;
      }
      case 6: // execute UI action
        onMenuClick(item.EventNumber, item.ParametersToPass);
        break;
      default:
        console.warn("Unknown action type: " + item.ActionType);
      // show global message?
    }
  };

  const onMenuClick = async (event: number, params: any = null) => {
    //TODO: Need to show busy indicator
    // maybe... (works for now but don't like it)

    //await onHomeClick();

    const url = "executeUIAction/" + event;

    const data = {
      Data: params || "",
    };

    const menuDetails = await api.makeApiCall<MenuDetail[]>(url, "POST", data);

    if (menuDetails && menuDetails.length > 0) {
      for (let i = 0; i < menuDetails?.length; i++) {
        handleAction(menuDetails[i]);
      }
    }
  };

  return { onMenuClick };
};
