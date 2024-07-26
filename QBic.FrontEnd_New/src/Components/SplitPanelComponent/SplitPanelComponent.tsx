import { SplitPanel, Tabs } from "@cloudscape-design/components";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";
import { useEffect, useState } from "react";
import { useApi } from "../../Hooks/apiHook";
import {
  ViewDetailComponent,
  ViewDetailSection,
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { TableComponent } from "../View/Table.component";

interface SplitPanelComponentProps {
  showContent: boolean;
}

export const SplitPanelComponent = ({
  showContent,
}: SplitPanelComponentProps) => {
  const mainApp = useMainApp();
  const api = useApi();

  const [detailComponents, setDetailComponents] = useState<
    ViewDetailComponent[]
  >([]);

  const [title, setTitle] = useState("");

  const loadComponentDetails = async () => {
    const resp = await api.makeApiCall<ViewDetailSection>(
      "getViewDetailSection/" + mainApp.selectedRow.menuItem.DetailSectionId,
      "POST",
      mainApp?.selectedRow?.rowData
    );

    setTitle(resp.Title);

    if (resp.Components) {
      setDetailComponents(resp.Components);
    } else {
      setDetailComponents([]);
    }

    //TODO: disable selection if no components are set
  };

  useEffect(() => {
    if (mainApp.selectedRow?.menuItem?.DetailSectionId) {
      loadComponentDetails();
    }
  }, [mainApp.selectedRow]);

  return mainApp.selectedRow == null || detailComponents.length == 0 ? null : (
    <SplitPanel header={title || "No item selected"}>
      <Tabs
        tabs={
          showContent == false
            ? []
            : detailComponents.map((x) => ({
                id: x.Id.toString(),
                label: x.Title,
                content: (
                  <TableComponent
                    isEmbedded={true}
                    defaultData={x.Data}
                    menuItem={{
                      ViewData: x.Data,
                      Columns: x.Columns,
                      Id: x.Id,
                      Title: x.Title,
                      TotalLines: -1,
                    }}
                  />
                ),
              }))
        }
      ></Tabs>
    </SplitPanel>
  );
};
