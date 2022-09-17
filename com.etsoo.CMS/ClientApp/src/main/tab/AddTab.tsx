import { ComboBox, EditPage, InputField } from '@etsoo/materialui';
import { FormControlLabel, Grid, Switch } from '@mui/material';
import React from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { DataTypes, Utils } from '@etsoo/shared';
import { IActionResult } from '@etsoo/appscript';
import { useNavigate } from 'react-router-dom';
import { app } from '../../app/MyApp';
import { TabUpdateDto } from '../../dto/TabUpdateDto';
import { TabSelector } from '../../components/TabSelector';
import { useParamsEx, useSearchParamsEx } from '@etsoo/react';
import { TabDto } from '../../dto/TabDto';

function AddTab() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: 'number' });

  const { parent } = useSearchParamsEx({ parent: 'number' });
  const [parents, setParents] = React.useState<number[]>();
  const currentTab = React.useRef<TabDto>();

  // Is editing
  const isEditing = id != null;
  type EditData = DataTypes.AddOrEditType<TabUpdateDto, typeof isEditing>;

  // Labels
  const labels = app.getLabels(
    'noChanges',
    'enabled',
    'id',
    'tab',
    'deleteConfirm',
    'tabName',
    'tabUrl',
    'tabLayout',
    'parentTab'
  );

  // Form validation schema
  const validationSchema = Yup.object({
    name: Yup.string().required(),
    url: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<EditData>({
    id,
    enabled: true,
    layout: 0
  });

  // Tab layouts
  const layouts = app.getTabLayouts();

  // Formik
  // https://formik.org/docs/examples/with-material-ui
  // https://firxworx.com/blog/coding/react/integrating-formik-with-react-material-ui-and-typescript/
  const formik = useFormik<EditData>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      // Correct for types safety
      Utils.correctTypes(rq, {});

      if (isEditing) {
        // Changed fields
        const fields: string[] = Utils.getDataChanges(rq, data);
        if (fields.length === 0) {
          app.warning(labels.noChanges);
          return;
        }
        rq.changedFields = fields;
      }

      // Submit
      const result = await app.api.put<IActionResult>(
        isEditing ? 'Tab/Update' : 'Tab/Create',
        rq
      );
      if (result == null) return;

      if (result.ok) {
        navigate(
          app.transformUrl(`/home/tab/all?parent=${values.parent ?? ''}`)
        );
        return;
      }

      app.alertResult(result);
    }
  });

  const handleBlur = (event: React.FocusEvent<HTMLInputElement>) => {
    if (!isEditing || formik.values.url === '') {
      const name = event.target.value;
      app.formatUrl(name).then((url) => {
        if (url == null) return;

        if (currentTab.current == null) {
          if (url === 'home' || url === 'fontpage') url = '';
          formik.setFieldValue('url', '/' + url);
        } else {
          formik.setFieldValue('url', currentTab.current.url + '/' + url);
        }
      });
    }
  };

  // Load data
  const loadData = async () => {
    if (id == null) return;
    app.api.get<TabUpdateDto>('Tab/UpdateRead/' + id).then((data) => {
      if (data == null) return;
      setData(data);

      if (data.parent != null) ancestorRead(data.parent);
    });
  };

  const ancestorRead = (parent: number) => {
    app.api.get<number[]>('Tab/AncestorRead/' + parent).then((data) => {
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
    app.setPageKey(isEditing ? 'editTab' : 'addTab');

    return () => {
      app.pageExit();
    };
  }, [isEditing]);

  return (
    <EditPage
      isEditing={isEditing}
      onDelete={
        (data?.articles ?? 1) > 0 || data?.url === '/'
          ? undefined
          : () => {
              app.notifier.confirm(
                labels.deleteConfirm.format(labels.tab),
                undefined,
                async (ok) => {
                  const id = formik.values.id;
                  if (!ok || id == null) return;

                  const result = await app.api.delete<IActionResult>(
                    `Tab/Delete/${id}`
                  );
                  if (result == null) return;

                  if (result.ok) {
                    navigate!(app.transformUrl('/home/tab/all'));
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
          onChange={(value) => formik.setFieldValue('parent', value)}
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
          value={formik.values.name ?? ''}
          onChange={formik.handleChange}
          onBlur={handleBlur}
          error={formik.touched.name && Boolean(formik.errors.name)}
          helperText={formik.touched.name && formik.errors.name}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="url"
          inputProps={{ maxLength: 128 }}
          label={labels.tabUrl}
          value={formik.values.url ?? ''}
          onChange={formik.handleChange}
          error={formik.touched.url && Boolean(formik.errors.url)}
          helperText={formik.touched.url && formik.errors.url}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <ComboBox
          options={layouts}
          name="layout"
          label={labels.tabLayout}
          idValue={formik.values.layout}
          inputRequired
          inputOnChange={(event) => formik.handleChange(event)}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
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
    </EditPage>
  );
}

export default AddTab;
