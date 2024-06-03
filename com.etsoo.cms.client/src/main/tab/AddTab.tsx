import {
  ComboBox,
  CustomFieldWindow,
  EditPage,
  InputField
} from "@etsoo/materialui";
import { Badge, Button, FormControlLabel, Grid, Switch } from "@mui/material";
import React from "react";
import { useFormik } from "formik";
import { DataTypes, IdActionResult, Utils } from "@etsoo/shared";
import { useNavigate } from "react-router-dom";
import { app } from "../../app/MyApp";
import { TabSelector } from "../../components/TabSelector";
import {
  ReactUtils,
  useParamsEx,
  useRefs,
  useSearchParamsEx
} from "@etsoo/react";
import { TabDto } from "../../api/dto/tab/TabDto";
import { TabUpdateDto } from "../../api/dto/tab/TabUpdateDto";
import SettingsIcon from "@mui/icons-material/Settings";
import { CustomFieldData } from "@etsoo/appscript";

function AddTab() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: "number" });

  const { parent } = useSearchParamsEx({ parent: "number" });
  const [parents, setParents] = React.useState<number[]>();
  const currentTab = React.useRef<TabDto>();

  const [customFields, setCustomField] = React.useState<CustomFieldData[]>([]);

  // Is editing
  const isEditing = id != null;
  type DataType = DataTypes.AddAndEditType<TabUpdateDto>;

  // Labels
  const labels = app.getLabels(
    "noChanges",
    "enabled",
    "id",
    "tab",
    "deleteConfirm",
    "tabName",
    "tabUrl",
    "tabLayout",
    "parentTab",
    "articleDescription",
    "tabLogo",
    "tabIcon",
    "noChildTab",
    "jsonData"
  );

  // Edit data
  const [data, setData] = React.useState<DataType>({
    name: "",
    url: "",
    enabled: true,
    articles: 0,
    layout: 0
  });

  // Tab layouts
  const layouts = app.getTabLayouts();

  // Input refs
  const refFields = [
    "name",
    "url",
    "logo",
    "description",
    "icon",
    "jsonData"
  ] as const;
  const refs = useRefs(refFields);

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<DataType>({
    initialValues: data,
    enableReinitialize: true,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      ReactUtils.updateRefValues(refs, rq);

      // Correct for types safety
      // Utils.correctTypes(rq, {});

      let result: IdActionResult | undefined;
      if (rq.id != null) {
        // Changed fields
        const fields: string[] = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;

        result = await app.tabApi.update(rq);
      } else {
        result = await app.tabApi.create(rq);
      }

      if (result == null) return;

      if (result.ok) {
        navigate(
          `${isEditing ? "./../../all" : "./../all"}?parent=${
            values.parent ?? ""
          }`
        );
        return;
      }

      app.alertResult(result);
    }
  });

  const handleBlur = (event: React.FocusEvent<HTMLInputElement>) => {
    const urlInput = refs.url.current;
    if (urlInput == null) return;
    if (!isEditing || urlInput.value === "") {
      const name = event.target.value;
      app.formatUrl(name).then((url) => {
        if (url == null) return;

        if (currentTab.current == null) {
          if (url === "home" || url === "fontpage") url = "";
          urlInput.value = "/" + url;
        } else {
          urlInput.value = currentTab.current.url + "/" + url;
        }
      });
    }
  };

  // Load data
  const loadData = async () => {
    if (id == null) return;
    app.api.get<TabUpdateDto>("Tab/UpdateRead/" + id).then((data) => {
      if (data == null) return;
      setData(data);

      ReactUtils.updateRefs(refs, data);

      if (data.parent != null) ancestorRead(data.parent);
    });
  };

  const ancestorRead = (parent: number) => {
    app.api.get<number[]>("Tab/AncestorRead/" + parent).then((data) => {
      if (data == null) return;
      setParents(data.reverse());
    });
  };

  React.useEffect(() => {
    if (parent) {
      ancestorRead(parent);
    }
  }, [parent]);

  React.useEffect(() => {
    // Page title
    app.setPageKey(isEditing ? "editTab" : "addTab");

    app.websiteApi.queryTabJsonDataSchema().then((schema) => {
      if (schema == null) return;
      setCustomField(JSON.parse(schema) as CustomFieldData[]);
    });

    return () => {
      app.pageExit();
    };
  }, [isEditing]);

  return (
    <EditPage
      isEditing={isEditing}
      onDelete={
        (data?.articles ?? 1) > 0 || data?.url === "/"
          ? undefined
          : () => {
              app.notifier.confirm(
                labels.deleteConfirm.format(labels.tab),
                undefined,
                async (ok) => {
                  const id = formik.values.id;
                  if (!ok || id == null) return;

                  const result = await app.tabApi.delete(id);
                  if (result == null) return;

                  if (result.ok) {
                    navigate("./../../all");
                    return;
                  }

                  app.alertResult(result);
                }
              );
            }
      }
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={loadData}
    >
      <Grid container item xs={12} sm={12} spacing={1}>
        <TabSelector
          name="parent"
          label={labels.parentTab}
          values={parents}
          onChange={(value) => formik.setFieldValue("parent", value)}
          onItemChange={(option) => (currentTab.current = option)}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="name"
          inputProps={{ maxLength: 64 }}
          label={labels.tabName}
          inputRef={refs.name}
          onBlur={handleBlur}
          disabled={currentTab.current?.url === "/"}
          helperText={
            currentTab.current?.url === "/" ? labels.noChildTab : undefined
          }
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="url"
          inputProps={{ maxLength: 128 }}
          inputRef={refs.url}
          label={labels.tabUrl}
          disabled={currentTab.current?.url === "/"}
        />
      </Grid>
      <Grid item xs={12} sm={5}>
        <ComboBox
          options={layouts}
          name="layout"
          label={labels.tabLayout}
          idValue={formik.values.layout}
          inputRequired
          inputOnChange={(event) => formik.handleChange(event)}
        />
      </Grid>
      <Grid item xs={12} sm={3}>
        <FormControlLabel
          control={
            <Switch
              name="enabled"
              checked={formik.values.enabled ?? true}
              onChange={formik.handleChange}
            />
          }
          label={labels.enabled}
        />
      </Grid>
      {customFields.length > 0 && (
        <Grid item xs={6} sm={4}>
          <CustomFieldWindow
            label={labels.jsonData}
            jsonData={data.jsonData}
            inputRef={refs.jsonData}
          >
            {(open, label, pc) => (
              <Button
                variant="outlined"
                fullWidth
                onClick={() => {
                  open(customFields);
                }}
                endIcon={
                  pc > 0 ? (
                    <Badge color="secondary" badgeContent={pc}>
                      <SettingsIcon />
                    </Badge>
                  ) : (
                    <SettingsIcon />
                  )
                }
              >
                {label}
              </Button>
            )}
          </CustomFieldWindow>
        </Grid>
      )}
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="logo"
          inputProps={{ maxLength: 256 }}
          inputRef={refs.logo}
          label={labels.tabLogo}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="icon"
          inputProps={{ maxLength: 128 }}
          inputRef={refs.icon}
          label={labels.tabIcon}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          multiline
          rows={2}
          name="description"
          inputProps={{ maxLength: 1024 }}
          inputRef={refs.description}
          label={labels.articleDescription}
        />
      </Grid>
    </EditPage>
  );
}

export default AddTab;
