import { MenuDetail } from "../../ContextProviders/MenuProvider/MenuProvider";
import { TableComponent } from "./Table.component";

interface ViewComponentProps {
  menuItem: MenuDetail;
}

export const ViewComponent = ({ menuItem }: ViewComponentProps) => {
  return <TableComponent menuItem={menuItem}></TableComponent>;
};
