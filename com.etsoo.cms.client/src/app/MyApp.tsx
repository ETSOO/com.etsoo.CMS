import {
  zhHans,
  zhHant,
  en,
  ApiAuthorizationScheme,
  AddressUtils,
  ExternalSettings,
  UserRole,
  LoginRQ
} from "@etsoo/appscript";
import {
  CommonApp,
  IServiceAppSettings,
  MUGlobal,
  TextFieldEx,
  TextFieldExMethods,
  VBox
} from "@etsoo/materialui";
import { DataTypes, DomUtils, DynamicActionResult, Utils } from "@etsoo/shared";
import React from "react";
import { AuditKind } from "../api/dto/AuditKind";
import {
  CoreConstants,
  INotificationReact,
  NotificationMessageType
} from "@etsoo/react";
import { TabLayout } from "../api/dto/tab/TabLayout";
import { ArticleLink } from "../api/dto/article/ArticleLink";
import { AuthApi } from "../api/AuthApi";
import { UserApi } from "../api/UserApi";
import { ArticleApi } from "../api/ArticleApi";
import { TabApi } from "../api/TabApi";
import { WebsiteApi } from "../api/WebsiteApi";
import { NavigateFunction } from "react-router-dom";
import { DriveApi } from "../api/DriveApi";

/**
 * Service App
 */
class MyServiceApp extends CommonApp {
  private triedCount = 0;
  private loginDialog?: INotificationReact;

  /**
   * Authorization API
   */
  readonly authApi = new AuthApi(this);

  /**
   * User API
   */
  readonly userApi = new UserApi(this);

  /**
   * Article API
   */
  readonly articleApi = new ArticleApi(this);

  /**
   * Tab API
   */
  readonly tabApi = new TabApi(this);

  /**
   * Website API
   */
  readonly websiteApi = new WebsiteApi(this);

  /**
   * Online drive API
   */
  readonly driveApi = new DriveApi(this);

  /**
   * Site domain
   */
  domain?: string;

  /**
   * Navigate function
   */
  navigateFn?: NavigateFunction;

  /**
   * Format article URL
   * 格式化文章链接
   * @param item Article link item
   * @returns Result
   */
  formatLink(item: ArticleLink) {
    const { url, tabLayout, tabUrl } = item;
    if (tabLayout === 0) return `${this.domain}${tabUrl}`;
    if (tabLayout === 1) return "#";
    return `${this.domain}${tabUrl}/${url}`;
  }

  private formatTitle(result: DynamicActionResult): [boolean, string] {
    let disabled: boolean = false;
    let title: string = result.title ?? "Unknown";

    switch (result.type) {
      case "UserFrozen":
        const frozenTime = new Date(result.data?.frozenTime);
        title = title.format(frozenTime.toLocaleString(this.culture));
        disabled = true;
        break;
    }

    return [disabled, title];
  }

  trimChars(url: string) {
    url = url
      .replace(/\s*&\s*/g, "-")
      .replace(/\s+/g, "-")
      .toLowerCase()
      .replace(/[^0-9a-z_-]+/g, "")
      .replace(/_{2,}/g, "_")
      .replace(/-{2,}/g, "-");
    return url;
  }

  async formatUrl(url: string) {
    if (url.trim() === "") return undefined;
    if (/^[\x20-\x7F]+$/.test(url)) return this.trimChars(url);

    const t = await this.translate(url);
    if (t == null) return undefined;
    return this.trimChars(t);
  }

  getLocalRoles() {
    return this.getRoles(UserRole.User | UserRole.Admin);
  }

  getAuditKinds() {
    return this.getEnumList(AuditKind, "ak");
  }

  getTabLayouts() {
    return this.getEnumList(TabLayout, "layout");
  }

  translate(text: string) {
    return this.articleApi.translate(text, { showLoading: false });
  }

  /**
   * Reset user password
   * @param id User id
   * @param callback Callback
   */
  async resetPassword(id: string, callback?: () => void) {
    const result = await this.userApi.resetPassword(id);
    if (result == null) return;

    if (result.ok && result.data) {
      var title =
        this.get<string>("newPassword") +
        ": " +
        this.decrypt(result.data.password);
      this.notifier.alert(title, callback, NotificationMessageType.Info);
      return;
    }

    this.alertResult(result);
  }

  /**
   * Go to the login page
   * @param tryLogin Try to login again
   */
  override toLoginPage(tryLogin?: boolean) {
    // Avoid repeated calls
    if (this.loginDialog?.open) return;

    // Try login
    // Dialog to receive password
    var labels = this.getLabels(
      "etsooCMS",
      "login",
      "password",
      "unknownError",
      "user"
    );

    const loginRef = React.createRef<HTMLInputElement>();
    const passwordRef = React.createRef<HTMLInputElement>();
    const mRef = React.createRef<TextFieldExMethods>();

    var loginCallback = async (
      form?: HTMLFormElement
    ): Promise<boolean | void> => {
      if (form == null) {
        return false;
      }

      // Form data
      const { username, password } = DomUtils.dataAs(new FormData(form), {
        username: "string",
        password: "string"
      });

      // Validate data
      if (username == null) {
        DomUtils.setFocus("username", form);
        return false;
      }

      if (password == null || !app.isValidPassword(password)) {
        DomUtils.setFocus("password", form);
        return false;
      }

      const data: LoginRQ = {
        id: this.encrypt(username),
        deviceId: this.deviceId,
        pwd: this.encrypt(this.hash(password)),
        region: this.region,
        timezone: this.getTimeZone()
      };

      const [result, refreshToken] = await this.authApi.login(data);

      if (result == null) {
        return false;
      }

      if (result.ok) {
        // Token
        if (refreshToken == null || result.data == null) {
          this.notifier.alert(labels.unknownError);
          return;
        }

        // Hold on cache
        this.storage.setPersistedData(CoreConstants.FieldUserIdSaved, username);

        // Default to keep
        var keep = true;

        // User login
        this.userLogin(result.data, refreshToken, keep);

        // Keep
        this.storage.setData(CoreConstants.FieldLoginKeep, keep);

        // Replace all pages
        if (this.navigateFn) this.navigateFn("/home");
        else app.navigate("/home/");

        // Delay
        setTimeout(() => this.loginDialog?.dismiss(), 0);

        return true;
      } else if (result.type === "DbConnectionFailed") {
        // Database created, try login again.
        // Avoid repeated trying
        if (this.triedCount === 0) {
          this.triedCount++;
          return await loginCallback(form);
        }
      }

      if (app.checkDeviceResult(result)) {
        this.initCall((result) => {
          if (result && this.loginDialog?.open) {
            loginCallback(form);
          }
        }, true);
        return false;
      }

      const [disabled, title] = this.formatTitle(result);

      var okButton = loginRef.current?.form?.elements.namedItem(
        "okButton"
      ) as HTMLButtonElement;
      if (okButton) okButton.disabled = disabled;
      mRef.current?.setError(title);

      return false;
    };

    this.loginDialog = this.showInputDialog({
      title: labels.login,
      message: `${labels.etsooCMS} (${import.meta.env.VITE_APP_VERSION})`,
      fullScreen: this.smDown,
      cancelButton: false,
      callback: async (form) => await loginCallback(form),
      inputs: (
        <VBox gap={1} width="100%">
          <TextFieldEx
            inputRef={(ref) => {
              Reflect.set(loginRef, "current", ref);

              const url = new URL(globalThis.location.href);

              const userIdSaved =
                url.searchParams.get("loginid") ??
                this.storage.getPersistedData<string>(
                  CoreConstants.FieldUserIdSaved
                );

              if (userIdSaved && loginRef.current)
                loginRef.current.value = userIdSaved;
            }}
            name="username"
            margin="dense"
            variant="standard"
            label={labels.user}
            autoComplete="username"
            onEnter={() => {
              if (loginRef.current?.value) {
                passwordRef.current?.focus();
              }
            }}
          />
          <TextFieldEx
            inputRef={passwordRef}
            ref={mRef}
            name="password"
            label={labels.password}
            variant="standard"
            showPassword
            autoComplete="current-password"
            onEnter={(event) =>
              event.currentTarget.closest("form")?.requestSubmit()
            }
          />
        </VBox>
      )
    });
  }
}

// Detected country or region
const { detectedCountry } = DomUtils;

// Detected culture
const { detectedCulture } = DomUtils;

// Global settings
MUGlobal.textFieldVariant = "standard";

const supportedCultures: DataTypes.CultureDefinition[] = [
  zhHans(() => import("../i18n/zh-Hans.json")),
  zhHant(() => import("../i18n/zh-Hant.json")),
  en(() => import("../i18n/en.json"))
];
const supportedRegions = ["CN"];

// External settings
const externalSettings = ExternalSettings.create();
if (externalSettings == null) {
  throw new Error("No external settings");
}

// Settings
const settings: IServiceAppSettings = {
  // Merge external configs first
  ...externalSettings,

  // Authorization scheme
  authScheme: ApiAuthorizationScheme.Bearer,

  // Detected culture
  detectedCulture,

  // Supported cultures
  cultures: supportedCultures,

  // Supported regions
  regions: supportedRegions,

  // Browser's time zone
  timeZone: Utils.getTimeZone(),

  /**
   * Current service id
   */
  serviceId: 6,

  // Current country or region
  currentRegion: AddressUtils.getRegion(
    supportedRegions,
    detectedCountry,
    detectedCulture
  ),

  // Current culture
  currentCulture: DomUtils.getCulture(supportedCultures, detectedCulture)[0]!
};

/**
 * Application
 */
export const app = new MyServiceApp(settings, "etsooCMS");
app.setupLogging(undefined, true);

/**
 * Notifier provider
 */
export const NotifierProvider = MyServiceApp.notifierProvider;
