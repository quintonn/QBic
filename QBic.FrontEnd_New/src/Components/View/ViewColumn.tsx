import { ViewColumn } from "../../ContextProviders/MenuProvider/MenuProvider";
import { showColumn } from "../../Utilities/viewUtils";

interface ViewColumnCellProps {
  rowData: any;
  column: ViewColumn;
}

export const ViewColumnCell = ({ rowData, column }: ViewColumnCellProps) => {
  let colVal = rowData[column.ColumnName];

  if (showColumn({ rowData, column })) {
    if (column.ColumnType == 1) {
      if (colVal == true) {
        colVal = column.TrueValueDisplay;
      } else if (colVal == false) {
        colVal = column.FalseValueDisplay;
      }
    }

    return colVal;
  } else {
    colVal = "";
  }

  return colVal;
};
