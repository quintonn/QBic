import {
  Alert,
  Button,
  Form,
  FormField,
  Input,
  SpaceBetween,
} from "@cloudscape-design/components";
import { useState } from "react";

interface InputFormProps {}

export const InputForm = ({}: InputFormProps) => {
  const [errorMessage, setErrorMessage] = useState("");

  // todo
  return (
    <form onSubmit={() => console.log("x")}>
      <Form
        variant="full-page"
        actions={
          <SpaceBetween direction="horizontal" size="xs">
            <Button formAction="none" variant="link">
              Cancel
            </Button>
            <Button variant="primary">Submit</Button>
          </SpaceBetween>
        }
      >
        <SpaceBetween direction="vertical" size="l">
          <FormField label="Username" errorText={"user is required"}>
            <Input
              onChange={({ detail: { value } }) => console.log("on change")}
              placeholder="Enter username"
              value={"12 563"}
            />
          </FormField>

          {errorMessage ? (
            <Alert
              statusIconAriaLabel="Error"
              type="error"
              header={`Error`}
              key="alert-item"
              dismissible
              onDismiss={() => setErrorMessage("")}
            >
              {errorMessage}
            </Alert>
          ) : null}
        </SpaceBetween>
      </Form>
    </form>
  );
};
