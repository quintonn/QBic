import {
  Box,
  Button,
  Modal,
  SpaceBetween,
} from "@cloudscape-design/components";
import { createContext, useContext, useState } from "react";
import { ViewEvent, useMenu } from "../MenuProvider/MenuProvider";
import { useAuth } from "../AuthProvider/AuthProvider";

//import { store } from "../app/store";

type ModalAction = "none" | "user-confirmation" | "unknown-remove-when-known";

interface ModalContextValue {
  getUserConfirmation: (settings: ViewEvent, data: any) => Promise<boolean>;
}

export const ModalContext = createContext<ModalContextValue>(
  {} as ModalContextValue
);

interface ModalContextProviderProps {
  children: React.ReactNode;
}

interface ModalDialogProps {
  onCloseAction: (result: boolean | null) => void;
}

export const ModalProvider = ({ children }: ModalContextProviderProps) => {
  const { currentMenu, onMenuClick } = useMenu();
  const auth = useAuth();

  const [isVisible, setIsVisible] = useState(false);

  const [currentAction, setCurrentAction] = useState<ModalAction>("none");
  const [confirmationSettings, setConfirmationSettings] =
    useState<ViewEvent>(null);

  const [mainProps, setMainProps] = useState<ModalDialogProps>({
    onCloseAction: () => {
      return Promise.resolve();
    },
  });

  // TODO: maybe todo. keep stack of modals so that they can be closed incrementally using an internal id etc.

  const getUserConfirmation = (
    props: ViewEvent,
    data: any
  ): Promise<boolean> => {
    setCurrentAction("user-confirmation");
    setConfirmationSettings(props);
    setIsVisible(true);

    return new Promise<any>((resolve) => {
      setMainProps({
        ...mainProps,
        onCloseAction: (result: boolean | null) => {
          setIsVisible(false);

          if (result === true && props.OnConfirmationUIAction > 0) {
            onMenuClick(props.OnConfirmationUIAction, data);
          } else if (result === false && props.OnCancelUIAction > 0) {
            onMenuClick(props.OnCancelUIAction, data);
          }

          resolve(result);
        },
      });
    });
  };

  const onDismiss = (result: boolean | null) => {
    mainProps.onCloseAction(result);
  };

  const UserConfirmationDialog = () => {
    return (
      <Modal
        onDismiss={(detail) => onDismiss(null)}
        visible={currentAction != "none" && isVisible}
        footer={
          <Box float="right">
            <SpaceBetween direction="horizontal" size="xs">
              <Button variant="link" onClick={() => onDismiss(false)}>
                {confirmationSettings?.CancelButtonText}
              </Button>
              <Button variant="primary" onClick={() => onDismiss(true)}>
                {confirmationSettings?.ConfirmationButtonText}
              </Button>
            </SpaceBetween>
          </Box>
        }
        header="Confirmation"
      >
        {confirmationSettings?.ConfirmationMessage}
      </Modal>
    );
  };

  return (
    <ModalContext.Provider value={{ getUserConfirmation }}>
      {currentAction == "user-confirmation" ? <UserConfirmationDialog /> : null}
      {children}
    </ModalContext.Provider>
  );
};

export const useModal = () => {
  const context = useContext(ModalContext);
  if (!context) {
    throw new Error("useModal must be used within an ModalProvider");
  }
  return context;
};
