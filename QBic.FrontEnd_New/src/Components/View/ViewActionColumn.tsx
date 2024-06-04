import { Box, Button, SpaceBetween } from "@cloudscape-design/components";
import {
  ColumnType,
  MenuDetail,
  ViewColumn,
  useMenu,
} from "../../ContextProviders/MenuProvider/MenuProvider";
import { showColumn } from "../../Utilities/viewUtils";

export interface ViewActionColumnProps {
  rowData: any[];
  columns: ViewColumn[];
  menu: MenuDetail;
}

const ColButton = ({
  rowData,
  column,
  menu,
}: {
  rowData: any;
  column: ViewColumn;
  menu: MenuDetail;
}) => {
  const visible = showColumn({ rowData, column });
  const { onMenuClick } = useMenu();

  let label = column.ColumnLabel;
  if (
    column.ColumnType == ColumnType.Button ||
    column.ColumnType == ColumnType.Link
  ) {
    label = column.LinkLabel;
  }

  return (
    <div
      style={{
        opacity: visible ? 1 : 0,
        display: "flex",
        height: "100%", // to make the link items align vertical center
      }}
    >
      <Button
        variant={
          column.ColumnType == ColumnType.Link ? "inline-link" : "normal"
        }
        onClick={() => {
          const id = rowData[column.KeyColumn];
          const params = column.ParametersToPass || {};
          params["ViewId"] = column.EventNumber;
          params["RowId"] = id;

          var formData = {
            Id: id,
            data: rowData,
            viewSettings: "", // Why is this not included in the call?
            //parameters: theColumn.ParametersToPass,
            parameters: params,
            eventParameters: menu.EventParameters,
          };

          // execute ui action
          onMenuClick(column.EventNumber, formData);
        }}
        wrapText={false}
        disabled={visible ? false : true} // this is so the cursor is normal when it's invisible
      >
        {label}
      </Button>
    </div>
  );
};

export const ViewActionColumn = ({
  rowData,
  columns,
  menu,
}: ViewActionColumnProps) => {
  if (!columns || columns.length == 0) {
    return "";
  }

  return (
    <Box float="right">
      <SpaceBetween direction="horizontal" size="xl">
        {columns
          // .filter((c) => showColumn({ rowData, column: c }))
          .map((c, i) => (
            <ColButton key={i} rowData={rowData} column={c} menu={menu} />
          ))}
        {/* <Button
        href="#"
        variant="link"
        onClick={() => {
          console.log(rowData);
        }}
      >
        Edit
      </Button>
      <Button>X</Button> */}
      </SpaceBetween>
    </Box>
  );
};
