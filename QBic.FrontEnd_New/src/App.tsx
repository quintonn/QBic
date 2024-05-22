import "@cloudscape-design/global-styles/index.css";

import { AuthContextProvider } from "./ContextProviders/AuthProvider/AuthContextProvider";
import { ApiContextProvider } from "./ContextProviders/ApiContextProvider/ApiContextProvider";
import { AppInfoContextProvider } from "./ContextProviders/AppInfoContextProvider/AppInfoContextProvider";
import { MainBody } from "./Components/MainBody/MainBody.component";

function App() {
  return (
    <AppInfoContextProvider>
      <AuthContextProvider>
        <ApiContextProvider>
          <MainBody />
        </ApiContextProvider>
      </AuthContextProvider>
    </AppInfoContextProvider>
  );
}

export default App;
