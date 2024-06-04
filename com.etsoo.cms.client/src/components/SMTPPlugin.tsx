import { Switch, TextFieldEx, VBox } from "@etsoo/materialui";
import { DomUtils } from "@etsoo/shared";
import { app } from "../app/MyApp";
import {
  checkSecret,
  loadPluginSecret,
  PluginItem,
  PluginProps
} from "./PluginItem";

/**
 * SMTPPlugin plugin
 * @returns Component
 */
export function SMTPPlugin(props: PluginProps) {
  const name = "SMTP";
  const secret = "******";
  const { initData, disabled } = props;
  const item = initData?.find((d) => d.id === name);

  const json = `{"host": "email-smtp.ap-southeast-1.amazonaws.com", "port": 465, "useSsl": true, "sender": "", "userName": "", "password": "", "to": null, "cc": null, "bcc": []}`;
  const labels = app.getLabels(
    "serviceSMTPApp",
    "serviceSMTPSecret",
    "enabled"
  );

  return (
    <PluginItem
      name={name}
      url="https://aws.amazon.com/"
      initData={item}
      disabled={disabled}
      inputs={(data) => (
        <VBox gap={1} marginTop={1}>
          <TextFieldEx
            name="app"
            label={labels.serviceSMTPApp}
            autoCorrect="off"
            defaultValue={data?.app ?? "SES"}
            showClear
            required
          />
          <TextFieldEx
            name="secret"
            label={labels.serviceSMTPSecret}
            multiline
            rows={5}
            autoCorrect="off"
            defaultValue={data?.app ? secret : json}
            onDoubleClick={() => navigator.clipboard.writeText(json)}
            id={item?.id}
            onVisibility={loadPluginSecret}
            showClear
            showPassword
            required
          />
          <Switch
            name="enabled"
            label={labels.enabled}
            checked={data?.enabled ?? true}
          />
        </VBox>
      )}
      validator={(form, data, editing) => {
        if (data.app.length < 3) {
          DomUtils.setFocus("app", form);
          return false;
        }

        if (
          data.secret == null ||
          (!editing && data.secret === secret) ||
          (data.secret !== secret && !checkSecret(data.secret))
        ) {
          DomUtils.setFocus("secret", form);
          return false;
        }

        if (editing && data.secret === secret) {
          data.secret = undefined;
        }
      }}
    />
  );
}
