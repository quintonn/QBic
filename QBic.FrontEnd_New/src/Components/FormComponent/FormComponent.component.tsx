import {
  Container,
  ContentLayout,
  Header,
  SpaceBetween,
} from "@cloudscape-design/components";
import { InputForm } from "../InputForm/InputForm.component";

export const FormComponent = () => {
  return (
    <ContentLayout
      header={
        <SpaceBetween size="m">
          <Header
            variant="h1"
            // description="This is a generic description used in the header."
            // actions={<Button variant="primary">Button</Button>}
          >
            Main Header ddd
          </Header>
        </SpaceBetween>
      }
    >
      <Container header={<Header variant="h2">Form Heading</Header>}>
        <InputForm></InputForm>
      </Container>
    </ContentLayout>
  );
};
