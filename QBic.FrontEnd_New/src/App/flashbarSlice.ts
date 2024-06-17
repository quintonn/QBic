import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { FlashbarProps } from "@cloudscape-design/components";
import { RootState } from "./store";
import { v4 as uuidv4 } from "uuid";

// Define a type for the slice state
interface FlashbarState {
  messages: FlashbarProps.MessageDefinition[];
}

// Define the initial state using that type
const initialState: FlashbarState = {
  messages: [],
};

export const flashbarSlice = createSlice({
  name: "error",
  // `createSlice` will infer the state type from the `initialState` argument
  initialState,
  reducers: {
    addMessage: (
      state: FlashbarState,
      action: PayloadAction<
        Omit<
          FlashbarProps.MessageDefinition,
          "id" | "onDismiss" | "dismissible" | "dismissLabel"
        >
      >
    ) => {
      const id = uuidv4();
      const newPayload: FlashbarProps.MessageDefinition = {
        ...action.payload,
        id,
        dismissible: true,
        dismissLabel: "Dismiss message",
      };
      state.messages = [...state.messages, newPayload];
    },
    removeMessage: (state: FlashbarState, action: PayloadAction<string>) => {
      state.messages = state.messages.filter(
        (message) => message.id !== action.payload
      );
    },
  },
});

export const { addMessage, removeMessage } = flashbarSlice.actions;

// Other code such as selectors can use the imported `RootState` type
export const selectedMessages = (state: RootState) => state.messages;

export default flashbarSlice.reducer;
