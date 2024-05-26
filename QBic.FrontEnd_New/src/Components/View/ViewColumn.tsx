import { ViewColumn } from "../../ContextProviders/MenuProvider/MenuProvider";
import { showColumn } from "../../Utilities/viewUtils";

interface ViewColumnCellProps {
  rowData: any;
  column: ViewColumn;
}

export const ViewColumnCell = ({ rowData, column }: ViewColumnCellProps) => {
  // todo: show/hide columns based on settings
  if (showColumn({ rowData, column })) {
    return rowData[column.ColumnName]?.toString();
  } else {
    return "";
  }
};
