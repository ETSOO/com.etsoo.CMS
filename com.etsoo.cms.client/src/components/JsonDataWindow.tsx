import { globalApp } from "@etsoo/materialui";
import { NotificationMessageType } from "@etsoo/react";
import { TextField } from "@mui/material";
import { MouseEventHandler } from "react";

export type JsonDataWindowProps = {
  children: (onClick: MouseEventHandler, label?: string) => React.ReactNode;
  label?: string;
};

export function JsonDataWindow(props: JsonDataWindowProps) {
  // Validate app
  const app = globalApp;
  if (app == null) {
    throw new Error("No globalApp");
  }

  const { children, label = app.get("jsonData") } = props;

  const inputs = <TextField fullWidth multiline rows={8} />;

  return children((event) => {
    app.notifier.alert(
      [undefined, label],
      undefined,
      NotificationMessageType.Info,
      { fullScreen: app.smDown, inputs }
    );
  }, label);
}
