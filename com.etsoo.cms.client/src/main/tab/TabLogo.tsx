import React from "react";
import {
  CommonPage,
  UserAvatarEditor,
  UserAvatarEditorToBlob
} from "@etsoo/materialui";
import { app } from "../../app/MyApp";
import { useLocation, useNavigate } from "react-router-dom";
import { useParamsEx } from "@etsoo/react";
import { LinearProgress } from "@mui/material";
import { LocalUtils } from "../../app/LocalUtils";
import { TabLogoDto } from "../../api/dto/tab/TabLogoDto";

function TabLogo() {
  // Route
  const navigate = useNavigate();
  const { id = 0 } = useParamsEx({ id: "number" });

  const location = useLocation();
  const state = location.state as TabLogoDto;

  // Labels
  const { tabLogo } = app.getLabels("tabLogo");

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
    form.append("logo", blob);

    var result = await app.tabApi.uploadLogo(id, form);
    if (result == null) return;

    // Refresh token to get the updated avatar
    navigate(`./../../all`);
  };

  React.useEffect(() => {
    app.websiteApi.readJsonData().then((result) => {
      if (result == null) return;
      const tabLogoSize = result.tabLogoSize ?? [1600, 600];
      setSize(tabLogoSize);
    });
  }, []);

  React.useEffect(() => {
    app.setPageTitle(tabLogo, state.name);
  }, [tabLogo, state]);

  return (
    <CommonPage>
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

export default TabLogo;
