import { EditPage, InputField } from '@etsoo/materialui';
import { Grid } from '@mui/material';
import React from 'react';
import { useFormik } from 'formik';
import * as Yup from 'yup';
import { DataTypes, DateUtils, Utils } from '@etsoo/shared';
import { IActionResult } from '@etsoo/appscript';
import { useNavigate } from 'react-router-dom';
import { app } from '../../app/MyApp';
import { TabSelector } from '../../components/TabSelector';
import { useParamsEx, useSearchParamsEx } from '@etsoo/react';
import { ArticleUpdateDto } from '../../dto/ArticleUpdateDto';
import { EOEditorEx } from '@etsoo/reacteditor';

function AddTab() {
  // Route
  const navigate = useNavigate();
  const { id } = useParamsEx({ id: 'number' });

  const { tab } = useSearchParamsEx({ tab: 'number' });
  const [tabs, setTabs] = React.useState<number[]>();

  // Is editing
  const isEditing = id != null;
  type EditData = DataTypes.AddOrEditType<ArticleUpdateDto, typeof isEditing>;

  // Labels
  const labels = app.getLabels(
    'noChanges',
    'enabled',
    'id',
    'tab',
    'deleteConfirm',
    'articleTitle',
    'articleSubtitle',
    'articleKeywords',
    'articleDescription',
    'articleUrl',
    'articleRelease'
  );

  // Form validation schema
  const validationSchema = Yup.object({
    title: Yup.string().required(),
    url: Yup.string().required()
  });

  // Edit data
  const [data, setData] = React.useState<EditData>({
    id,
    title: '',
    content: '',
    release: new Date()
  });

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

      if (rq.tab1 == null) {
      }

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
        isEditing ? 'Article/Update' : 'Article/Create',
        rq
      );
      if (result == null) return;

      if (result.ok) {
        navigate(app.transformUrl(`/home/article/all`));
        return;
      }

      app.alertResult(result);
    }
  });

  const handleBlur = (event: React.FocusEvent<HTMLInputElement>) => {
    if (!isEditing || formik.values.url === '') {
      const title = event.target.value;
      app.translate(title).then((result) => {
        if (result == null) return;
        const url = app.formatUrl(result);
        formik.setFieldValue('url', url);
      });
    }
  };

  // Load data
  const loadData = async () => {
    if (id == null) return;
    app.api.get<ArticleUpdateDto>('Tab/UpdateRead/' + id).then((data) => {
      if (data == null) return;
      setData(data);

      if (data.tab1 != null) ancestorRead(data.tab1);
    });
  };

  const ancestorRead = (tab: number) => {
    app.api.get<number[]>('Tab/AncestorRead/' + tab).then((data) => {
      if (data == null) return;
      data.reverse();
      setTabs(data);
    });
  };

  React.useEffect(() => {
    if (tab) {
      ancestorRead(tab);
    }
  }, [tab]);

  React.useEffect(() => {
    // Page title
    app.setPageKey(isEditing ? 'editArticle' : 'addArticle');

    return () => {
      app.pageExit();
    };
  }, []);

  return (
    <EditPage
      isEditing={isEditing}
      onDelete={() => {
        app.notifier.confirm(
          labels.deleteConfirm.format(labels.tab),
          undefined,
          async (ok) => {
            const id = formik.values.id;
            if (!ok || id == null) return;

            const result = await app.api.delete<IActionResult>(
              `Article/Delete/${id}`
            );
            if (result == null) return;

            if (result.ok) {
              navigate!(app.transformUrl('/home/article/all'));
              return;
            }

            app.alertResult(result);
          }
        );
      }}
      onSubmit={(event) => {
        formik.handleSubmit(event);
      }}
      onUpdate={loadData}
    >
      <Grid container item xs={12} sm={12} spacing={1}>
        <TabSelector
          name="tab1"
          label={labels.tab}
          values={tabs}
          onChange={(value) => formik.setFieldValue('tab1', value)}
          required
          error={formik.touched.tab1 && Boolean(formik.errors.tab1)}
          helperText={formik.touched.tab1 && formik.errors.tab1}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="title"
          inputProps={{ maxLength: 256 }}
          label={labels.articleTitle}
          value={formik.values.title}
          onChange={formik.handleChange}
          onBlur={handleBlur}
          error={formik.touched.title && Boolean(formik.errors.title)}
          helperText={formik.touched.title && formik.errors.title}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="subtitle"
          inputProps={{ maxLength: 256 }}
          label={labels.articleSubtitle}
          value={formik.values.subtitle ?? ''}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          required
          name="url"
          inputProps={{ maxLength: 128 }}
          label={labels.articleUrl}
          value={formik.values.url ?? ''}
          onChange={formik.handleChange}
          error={formik.touched.url && Boolean(formik.errors.url)}
          helperText={formik.touched.url && formik.errors.url}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <EOEditorEx
          content={formik.values.content ?? ''}
          onBackup={(content) => formik.setFieldValue('content', content)}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          multiline
          rows={2}
          name="description"
          inputProps={{ maxLength: 512 }}
          label={labels.articleDescription}
          value={formik.values.description ?? ''}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={12}>
        <InputField
          fullWidth
          name="keywords"
          inputProps={{ maxLength: 256 }}
          label={labels.articleKeywords}
          value={formik.values.keywords ?? ''}
          onChange={formik.handleChange}
        />
      </Grid>
      <Grid item xs={12} sm={6}>
        <InputField
          fullWidth
          required
          name="release"
          type="datetime-local"
          label={labels.articleRelease}
          defaultValue={DateUtils.formatForInput(formik.values.release, true)}
          onChange={formik.handleChange}
          error={formik.touched.release && Boolean(formik.errors.release)}
          helperText={
            formik.touched.release && new String(formik.errors.release)
          }
        />
      </Grid>
    </EditPage>
  );
}

export default AddTab;
