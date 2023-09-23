import React from 'react';
import {
  CommonPage,
  UserAvatarEditor,
  UserAvatarEditorToBlob
} from '@etsoo/materialui';
import { app } from '../../app/MyApp';
import { useLocation, useNavigate } from 'react-router-dom';
import { useParamsEx } from '@etsoo/react';
import { Button, LinearProgress, Stack, Typography } from '@mui/material';
import { LocalUtils } from '../../app/LocalUtils';
import CollectionsIcon from '@mui/icons-material/Collections';
import { ArticleLogoDto } from '../../api/dto/article/ArticleLogoDto';

function ArticleLogo() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: 'number' });

  const location = useLocation();
  const state = location.state as ArticleLogoDto;

  // Labels
  const { articleTitle, articleLogo, slideshowLogo } = app.getLabels(
    'articleTitle',
    'articleLogo',
    'slideshowLogo'
  );

  // State
  const [size, setSize] = React.useState<[number, number]>();

  const handleDone = async (
    canvas: HTMLCanvasElement,
    toBlob: UserAvatarEditorToBlob,
    type: string
  ) => {
    // Photo blob
    const blob = await toBlob(canvas, type, 1);

    // Form data
    const form = new FormData();
    form.append('logo', blob);

    var result = await app.articleApi.uploadLogo(id, form);
    if (result == null) return;

    // Refresh token to get the updated avatar
    navigate(`./../../all`);
  };

  React.useEffect(() => {
    app.websiteApi.readJsonData().then((result) => {
      if (result == null) return;
      const logoSize = result.logoSize ?? [800, 600];
      setSize(logoSize);
    });
  }, []);

  React.useEffect(() => {
    app.setPageTitle(articleLogo);
  }, [articleLogo]);

  return (
    <CommonPage>
      <Stack alignItems="center" gap={2} direction="row" marginBottom={1}>
        <Button
          variant="outlined"
          startIcon={<CollectionsIcon />}
          onClick={() => navigate(`./../../gallery/${id}`, { state })}
        >
          {slideshowLogo}
        </Button>
        <Typography>
          {articleTitle}: {state.title}
        </Typography>
      </Stack>
      {size == null ? (
        <LinearProgress />
      ) : (
        <UserAvatarEditor
          onDone={handleDone}
          image={state.logo}
          {...LocalUtils.formatEditorSize(size)}
        />
      )}
    </CommonPage>
  );
}

export default ArticleLogo;
