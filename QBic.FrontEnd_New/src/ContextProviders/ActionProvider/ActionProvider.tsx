import { createContext, useContext } from "react";
import { MenuDetail, ViewEvent } from "../MenuProvider/MenuProvider";
import { useNavigate } from "react-router-dom";
import { useModal } from "../ModalProvider/ModalProvider";
import { useApi } from "../../Hooks/apiHook";
import { useMainApp } from "../MainAppProvider/MainAppProvider";
import { useAuth } from "../AuthProvider/AuthProvider";
import { store } from "../../App/store";
import { addMessage } from "../../App/flashbarSlice";
import { API_URL } from "../../Constants/AppValues";

interface ActionContextType {
  handleAction: (item: MenuDetail) => void;
  onMenuClick: (event: number, params?: any) => Promise<void>;
}

const ActionContext = createContext<ActionContextType>(null);

export const ActionProvider = ({ children }) => {
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
        Authorization: "Bearer " + auth.getAccessToken(),
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
        mainApp.setCacheValue("/view/" + item.Id, item);
        navigate("/view/" + item.Id);
        break;
      }
      case 1: {
        // get inputs
        //mainApp.setCurrentContentType("form");
        mainApp.setCacheValue("/form/" + item.Id, item);
        //mainApp.setCacheValue("/view/" + item.Id, item);
        navigate("/form/" + item.Id);
        break;
      }
      //case 2: // submenu  - // nothing, this is commented out in the old code
      //case 3: // do something - // nothing, this is commented out in the old code
      case 4: {
        // close input dialog (not sure if i need this here);
        // maybe navigate back
        navigate(-1);
        break;
      }
      case 5: {
        // ShowMessage
        modal.getUserConfirmation(item as ViewEvent, null).then((result) => {
          if (result === true && item.OnConfirmationUIAction > 0) {
            onMenuClick(item.OnConfirmationUIAction, null);
          } else if (result === false && item.OnCancelUIAction > 0) {
            onMenuClick(item.OnCancelUIAction, null);
          }
        });
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

  const value = {
    handleAction,
    onMenuClick,
  };

  return (
    <ActionContext.Provider value={value}>{children}</ActionContext.Provider>
  );
};

export const useActions = () => {
  const context = useContext(ActionContext);
  if (!context) {
    throw new Error("useActions must be used within an ActionProvider");
  }
  return context;
};
