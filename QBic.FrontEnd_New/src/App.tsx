import "@cloudscape-design/global-styles/index.css";

import { MainBody } from "./Components/MainBody/MainBody.component";
import { MainAppProvider } from "./ContextProviders/MainAppProvider/MainAppProvider";
import { Provider } from "react-redux";
import { store } from "./App/store";

function App() {
  return (
    <Provider store={store}>
      <MainAppProvider>
        <MainBody />
      </MainAppProvider>
    </Provider>
  );
}

export default App;
