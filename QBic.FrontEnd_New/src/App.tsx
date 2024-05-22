import "@cloudscape-design/global-styles/index.css";

import { MainBody } from "./Components/MainBody/MainBody.component";
import { MainAppProvider } from "./ContextProviders/MainAppProvider/MainAppProvider";

function App() {
  return (
    <MainAppProvider>
      <MainBody />
    </MainAppProvider>
  );
}

export default App;
