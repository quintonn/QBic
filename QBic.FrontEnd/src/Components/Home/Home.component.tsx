import { Box, ContentLayout, Grid } from "@cloudscape-design/components";
import { useMainApp } from "../../ContextProviders/MainAppProvider/MainAppProvider";

interface ServiceFeatureListItemProps {
  title: string;
  description: string;
}
const ServiceFeatureListItem = ({
  title,
  description,
}: ServiceFeatureListItemProps) => (
  <div>
    <Box variant="h3" padding="n" fontWeight="normal">
      {title}
    </Box>
    <Box variant="p">{description}</Box>
  </div>
);

export const Home = () => {
  const mainApp = useMainApp();
  return (
    <ContentLayout
      header={
        <Grid
          gridDefinition={[
            { colspan: { default: 12, xs: 8, s: 9 } }, // Service summary text
            { colspan: { default: 12, xs: 4, s: 3 } }, // Getting started action card
          ]}
        >
          <Box padding="s">
            <Box
              fontSize="display-l"
              fontWeight="bold"
              variant="h1"
              padding="n"
            >
              {mainApp.appName}
            </Box>
            HOME
            {/* <Box fontSize="display-l" fontWeight="light">
              Sub heading
            </Box>
            <Box
              margin={{ top: "xs", bottom: "l" }}
              color="text-body-secondary"
            >
              Summary
            </Box> */}
          </Box>
          {/* <Container
            header={<Header variant="h2">Manage stuff from here</Header>}
          >
            <SpaceBetween size="s">
              <Box variant="p">To get started, select a menu action</Box>
              <Button variant="primary">Sign in to get started</Button>
            </SpaceBetween>
          </Container> */}
        </Grid>
      }
    >
      {/* <Container
        header={
          <Box variant="h2" fontSize="heading-xl" padding="n">
            Features
          </Box>
        }
      >
        <SpaceBetween size="l" direction="vertical">
          <ServiceFeatureListItem
            key="item1"
            title="Feature 1"
            description="This is a cool feature"
          />
          <ServiceFeatureListItem
            key="item2"
            title="Feature 2"
            description="And something else"
          />
          <Link to="/test">Test</Link>
        </SpaceBetween>
      </Container> */}
    </ContentLayout>
  );
};
