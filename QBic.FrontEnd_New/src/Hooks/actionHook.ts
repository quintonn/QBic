import { useNavigate } from "react-router-dom";
import {
  MenuDetail,
  ViewEvent,
} from "../ContextProviders/MenuProvider/MenuProvider";
import { useModal } from "../ContextProviders/ModalProvider/ModalProvider";
import { useApi } from "./apiHook";
import { useMainApp } from "../ContextProviders/MainAppProvider/MainAppProvider";

import { API_URL } from "../Constants/AppValues";
import { useAuth } from "../ContextProviders/AuthProvider/AuthProvider";
import { request } from "http";
import { store } from "../App/store";
import { addMessage } from "../App/flashbarSlice";

export const useActions = () => {
  const navigate = useNavigate();
  const modal = useModal();
  const api = useApi();
  const mainApp = useMainApp();
  const auth = useAuth();

  const encode = (str: string) => {
    return btoa(
      encodeURIComponent(str).replace(/%([0-9A-F]{2})/g, function (match, p1) {
        return String.fromCharCode(0 + p1);
      })
    );
  };

  const downloadFile = async (url: string, requestData?: string) => {
    store.dispatch(
      addMessage({
        type: "info",
        content: "File download started...",
      })
    );

    let urlToCall = `${API_URL}${url}?v=${mainApp.appVersion}`;
    urlToCall = urlToCall.replaceAll("//", "/");

    // make API call
    const fetchOptions: RequestInit = {
      method: "POST",
      headers: {
        Authorization: "Bearer " + auth.accessToken,
      },
    };

    let dataToSend = "";
    if (requestData && requestData.length > 0) {
      fetchOptions.body = encode(dataToSend);
    }

    try {
      const response = await fetch(urlToCall, fetchOptions);

      if (response.ok) {
        const blob = await response.blob();

        let filename = response.headers.get("FileName");

        if (filename == null || filename.length == 0) {
          filename = "unknown file.zip";
        }

        const objUrl = URL.createObjectURL(new Blob([blob]));
        const link = document.createElement("a");
        link.href = objUrl;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        URL.revokeObjectURL(objUrl);

        link.remove();

        store.dispatch(
          addMessage({
            type: "success",
            content: "File download complete",
          })
        );
      }
    } catch (err) {
      console.log("error downloading file with url: " + urlToCall);
      console.log(err);
      store.dispatch(
        addMessage({
          type: "error",
          content: `Error downloading file: ${err}`,
        })
      );
    }
  };

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
      case 1: {
        // TODO: get user input
        // this is just about the next thing to do
        console.log("todo: get user inputs");
        break;
      }
      //case 2: // submenu  - // nothing, this is commented out in the old code
      //case 3: // do something - // nothing, this is commented out in the old code
      case 4: {
        // close input dialog (not sure if i need this here);
        console.log("close input dialog action ignored");
        break;
      }
      case 5: {
        // ShowMessage
        modal.getUserConfirmation(item as ViewEvent, null);
        break;
      }
      case 6: // execute UI action
        onMenuClick(item.EventNumber, item.ParametersToPass);
        break;
      //case 7: // input data view (ignore in old code)
      case 8: // update input view
      case 9: {
        // delete input view item
        console.log("todo: DeleteInputViewItem");
        break;
      }
      case 11: // download file
        downloadFile(item.DataUrl, item.RequestData);
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
