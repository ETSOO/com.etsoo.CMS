import { EditPage, InputField } from '@etsoo/materialui';
import * as Yup from 'yup';
import { useFormik } from 'formik';
import React from 'react';
import { app } from '../../app/MyApp';
import { IActionResult, UserRole } from '@etsoo/appscript';
import { DataTypes, Utils } from '@etsoo/shared';
import { useNavigate } from 'react-router-dom';
import { SettingsUpdateDto } from '../../dto/SettingsUpdateDto';
import { Grid } from '@mui/material';

function Settings() {
  // Navigate
  const navigate = useNavigate();

  // Labels
  const labels = app.getLabels(
    'operationSucceeded',
    'noChanges',
    'websiteDomain',
    'websiteTitle',
    'websiteDescription',
    'websiteKeywords',
    'websiteKeywordsTip'
  );

  // Form validation schema
  const validationSchema = Yup.object({
    title: Yup.string().required()
  });

  // Permissions
  const adminPermission = app.hasPermission([UserRole.Admin, UserRole.Founder]);

  // Edit data
  type EditData = DataTypes.AddOrEditType<SettingsUpdateDto, true>;
  const [data, setData] = React.useState<EditData>({
    domain: 'https://',
    title: ''
  });

  // Formik
  const formik = useFormik<EditData>({
    initialValues: data,
    enableReinitialize: true,
    validationSchema: validationSchema,
    onSubmit: async (values) => {
      // Request data
      const rq = { ...values };

      // Auto append http protocol
      if (!rq.domain.startsWith('http')) {
        rq.domain = 'http://' + rq.domain;
      }

      // Changed fields
      const fields: string[] = Utils.getDataChanges(rq, data);
      if (fields.length === 0) {
        app.warning(labels.noChanges);
        return;
      }
      rq.changedFields = fields;

      // Submit
      const result = await app.api.put<IActionResult>(
        'Website/UpdateSettings',
        rq
      );
      if (result == null) return;

      app.notifier.succeed(labels.operationSucceeded, undefined, () => {
        navigate('./../../');
      });
    }
  });

  // Load data
  const loadData = async () => {
    app.api.get<EditData>('Website/ReadSettings').then((data) => {
      if (data == null) return;
      setData(data);
    });
  };

  React.useEffect(() => {
    // Page title
    app.setPageKey('websiteSettings');

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
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="domain"
          inputProps={{ maxLength: 128 }}
          label={labels.websiteDomain}
          value={formik.values.domain}
          onChange={formik.handleChange}
          error={formik.touched.domain && Boolean(formik.errors.domain)}
          helperText={formik.touched.domain && formik.errors.domain}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="title"
          multiline
          rows={2}
          inputProps={{ maxLength: 128 }}
          label={labels.websiteTitle}
          value={formik.values.title}
          onChange={formik.handleChange}
          error={formik.touched.title && Boolean(formik.errors.title)}
          helperText={formik.touched.title && formik.errors.title}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="description"
          multiline
          rows={2}
          inputProps={{ maxLength: 512 }}
          label={labels.websiteDescription}
          value={formik.values.description ?? ''}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="keywords"
          multiline
          rows={2}
          inputProps={{ maxLength: 512 }}
          label={labels.websiteKeywords}
          value={formik.values.keywords ?? ''}
          onChange={formik.handleChange}
          helperText={labels.websiteKeywordsTip}
        />
      </Grid>
    </EditPage>
  );
}

export default Settings;
