import { useActions } from "../ContextProviders/ActionProvider/ActionProvider";
import {
  MenuDetail,
  ViewColumn,
} from "../ContextProviders/MenuProvider/MenuProvider";
import { useModal } from "../ContextProviders/ModalProvider/ModalProvider";

export const useViewEvents = () => {
  const { onMenuClick } = useActions();
  const modal = useModal();

  const handleViewEvent = (
    column: ViewColumn,
    rowData: any,
    menu: MenuDetail
  ) => {
    const id = rowData[column.KeyColumn];
    const params = column.ParametersToPass || {};
    params["ViewId"] = menu.Id;
    params["RowId"] = id;

    const formData = {
      Id: id,
      data: rowData,
      viewSettings: "", // Why is this not included in the call?
      //parameters: theColumn.ParametersToPass,
      parameters: params,
      eventParameters: menu.EventParameters,
    };

    if (menu.ActionType == 7) {
      console.log("TODO: addviewdatatoparams whatever that is");
      alert("todo: addviewdatatoparams");
    } else if (column.Event == null || column.Event.ActionType == 6) {
      const eventId =
        column.Event == null ? column.EventNumber : column.Event.EventNumber;
      // execute ui action

      onMenuClick(eventId, formData);
    } else if (column.Event.ActionType == 5) {
      // show message
      const data = formData || {};
      data["parameters"] = params;
      modal.getUserConfirmation(column.Event, data).then((res) => {
        if (res === true && column.Event.OnConfirmationUIAction > 0) {
          onMenuClick(column.Event.OnConfirmationUIAction, data);
        } else if (res === false && column.Event.OnCancelUIAction > 0) {
          onMenuClick(column.Event.OnCancelUIAction, data);
        }
      });
    }
  };

  return { handleViewEvent };
};