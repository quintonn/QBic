import { ViewColumn } from "../../ContextProviders/MenuProvider/MenuProvider";
import { showColumn } from "../../Utilities/viewUtils";

interface ViewColumnCellProps {
  rowData: any;
  column: ViewColumn;
}

const getNestedValue = (data, path) => {
  return path
    .split(".")
    .reduce((partValue, subPath) => partValue && partValue[subPath], data);
};

export const ViewColumnCell = ({ rowData, column }: ViewColumnCellProps) => {
  let colVal = getNestedValue(rowData, column.ColumnName);

  if (showColumn({ rowData, column })) {
    if (column.ColumnType == 1) {
      if (colVal == true) {
        colVal = column.TrueValueDisplay;
      } else if (colVal == false) {
        colVal = column.FalseValueDisplay;
      }
    }
  } else {
    colVal = "";
  }

  return colVal + ""; // else booleans don't show
};
