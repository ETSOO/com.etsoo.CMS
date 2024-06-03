import { EditPage, InputField, VBox } from "@etsoo/materialui";
import { useFormik } from "formik";
import React from "react";
import { app } from "../../app/MyApp";
import { UserRole } from "@etsoo/appscript";
import { DataTypes, DomUtils, Utils } from "@etsoo/shared";
import { useNavigate } from "react-router-dom";
import { SettingsUpdateDto } from "../../api/dto/website/SettingsUpdateDto";
import { Button, Grid } from "@mui/material";
import { ReactUtils, useRefs } from "@etsoo/react";

function Settings() {
  // Navigate
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    "operationSucceeded",
    "noChanges",
    "websiteDomain",
    "websiteTitle",
    "websiteDescription",
    "websiteKeywords",
    "websiteKeywordsTip",
    "articleLogoSize",
    "tabLogoSize",
    "slideshowLogoSize",
    "updateResourceUrl",
    "resourceUrl",
    "oldResourceUrl",
    "resourceUrlTip",
    "example"
  );

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  // Data type
  type DataType = DataTypes.AddOrEditType<SettingsUpdateDto, true, never>;
  const [data, setData] = React.useState<DataType>({
    domain: "https://",
    title: ""
  });

  // Input refs
  const refFields = [
    "domain",
    "title",
    "description",
    "keywords",
    "logoSize",
    "tabLogoSize",
    "galleryLogoSize"
  ] as const;
  const refs = useRefs(refFields);

  // Formik
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    onSubmit: async (values) => {
      // Request data
      // structuredClone to deal with the jsonData object
      const rq = structuredClone(values);

      ReactUtils.updateRefValues(refs, rq);

      const jsonData = rq.jsonData;
      if (jsonData != null && typeof jsonData === "object") {
        if (typeof jsonData.logoSize === "string") {
          if (jsonData.logoSize === "") {
            delete jsonData.logoSize;
          } else {
            const logoSize = Utils.parseJsonArray(jsonData.logoSize, 0);
            if (logoSize == null || logoSize.length < 2) {
              DomUtils.setFocus("jsonData.logoSize");
              return;
            }
            jsonData.logoSize = logoSize as [number, number];
          }
        }

        if (typeof jsonData.tabLogoSize === "string") {
          if (jsonData.tabLogoSize === "") {
            delete jsonData.tabLogoSize;
          } else {
            const tabLogoSize = Utils.parseJsonArray(jsonData.tabLogoSize, 0);
            if (tabLogoSize == null || tabLogoSize.length < 2) {
              DomUtils.setFocus("jsonData.tabLogoSize");
              return;
            }
            jsonData.tabLogoSize = tabLogoSize as [number, number];
          }
        }

        if (typeof jsonData.galleryLogoSize === "string") {
          if (jsonData.galleryLogoSize === "") {
            delete jsonData.galleryLogoSize;
          } else {
            const galleryLogoSize = Utils.parseJsonArray(
              jsonData.galleryLogoSize,
              0
            );
            if (galleryLogoSize == null || galleryLogoSize.length < 2) {
              DomUtils.setFocus("jsonData.galleryLogoSize");
              return;
            }
            jsonData.galleryLogoSize = galleryLogoSize as [number, number];
          }
        }
      }

      // Auto append http protocol
      if (rq.domain && !rq.domain.startsWith("http")) {
        rq.domain = "http://" + rq.domain;
      }

      // Changed fields
      const fields: string[] = Utils.getDataChanges(rq, data);
      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Submit
      const { jsonData: jsonDataObj, ...rest } = rq;
      const result = await app.websiteApi.updateSettings({
        jsonData: JSON.stringify(jsonDataObj),
        ...rest
      });
      if (result == null) return;

      if (result.ok) {
        app.notifier.succeed(labels.operationSucceeded, undefined, () => {
          navigate("./../../");
        });
        return;
      }

      app.alertResult(result);
    }
  });

  // Load data
  const loadData = async () => {
    const data = await app.websiteApi.readSettings();
    if (data == null) return;

    ReactUtils.updateRefs(refs, data);

    setData(data);
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey("websiteSettings");

    return () => {
      app.pageExit();
    };
  }, []);

  return (
    <EditPage
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={loadData}
      submitDisabled={!adminPermission}
    >
      <Grid item xs={12} sm={10}>
        <InputField
          fullWidth
          required
          name="domain"
          inputProps={{ maxLength: 128 }}
          inputRef={refs.domain}
          label={labels.websiteDomain}
        />
      </Grid>
      {data.rootUrl && (
        <Grid item xs={12} sm={2}>
          <Button
            fullWidth
            variant="outlined"
            onClick={() => {
              app.showInputDialog({
                title: labels.updateResourceUrl,
                message: undefined,
                fullScreen: app.smDown,
                callback: async (form) => {
                  // Cancelled
                  if (form == null) {
                    return;
                  }

                  // Form data
                  const { oldResourceUrl } = DomUtils.dataAs(
                    new FormData(form),
                    {
                      oldResourceUrl: "string"
                    }
                  );

                  if (
                    !oldResourceUrl ||
                    !oldResourceUrl.startsWith("http") ||
                    !oldResourceUrl.includes("://") ||
                    oldResourceUrl.length < 12
                  ) {
                    DomUtils.setFocus("oldResourceUrl", form);
                    return false;
                  }

                  // Submit
                  const result = await app.websiteApi.updateResourceUrl(
                    oldResourceUrl
                  );
                  if (result == null) return;

                  if (result.ok) {
                    app.notifier.succeed(labels.operationSucceeded);
                    return;
                  }

                  return app.formatResult(result);
                },
                inputs: (
                  <VBox gap={2} marginTop={2}>
                    <InputField
                      fullWidth
                      name="oldResourceUrl"
                      required
                      inputProps={{ maxLength: 128 }}
                      label={labels.oldResourceUrl}
                      defaultValue="https://"
                      helperText={labels.resourceUrlTip}
                    />
                    <InputField
                      fullWidth
                      name="resorceUrl"
                      required
                      inputProps={{ maxLength: 128 }}
                      label={labels.resourceUrl}
                      defaultValue={data.rootUrl}
                      disabled
                    />
                  </VBox>
                )
              });
            }}
          >
            {labels.updateResourceUrl}
          </Button>
        </Grid>
      )}
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="title"
          multiline
          rows={2}
          inputProps={{ maxLength: 128 }}
          inputRef={refs.title}
          label={labels.websiteTitle}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="description"
          multiline
          rows={4}
          inputProps={{ maxLength: 1024 }}
          inputRef={refs.description}
          label={labels.websiteDescription}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="keywords"
          multiline
          rows={2}
          inputProps={{ maxLength: 512 }}
          inputRef={refs.keywords}
          label={labels.websiteKeywords}
        />
      </Grid>
      <Grid item xs={12} md={4}>
        <InputField
          fullWidth
          name="jsonData.logoSize"
          inputRef={refs.logoSize}
          label={labels.articleLogoSize}
          helperText={`${labels.example}: 800, 600`}
        />
      </Grid>
      <Grid item xs={12} md={4}>
        <InputField
          fullWidth
          name="jsonData.tabLogoSize"
          inputRef={refs.tabLogoSize}
          label={labels.tabLogoSize}
          helperText={`${labels.example}: 1600, 600`}
        />
      </Grid>
      <Grid item xs={12} md={4}>
        <InputField
          fullWidth
          name="jsonData.galleryLogoSize"
          inputRef={refs.galleryLogoSize}
          label={labels.slideshowLogoSize}
          helperText={`${labels.example}: 800, 0`}
        />
      </Grid>
    </EditPage>
  );
}

export default Settings;
