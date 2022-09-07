import {
  zhCN,
  zhHK,
  enUS,
  ApiAuthorizationScheme,
  AddressUtils,
  ExternalSettings,
  IApiPayload,
  LoginRQ,
  DynamicActionResult,
  UserRole,
  IActionResult
} from '@etsoo/appscript';
import {
  CommonApp,
  IServiceAppSettings,
  MUGlobal,
  ServiceLoginResult,
  TextFieldEx,
  TextFieldExMethods,
  VBox
} from '@etsoo/materialui';
import { DataTypes, DomUtils, Utils } from '@etsoo/shared';
import zhCNResources from '../i18n/zh-CN.json';
import zhHKResources from '../i18n/zh-HK.json';
import enUSResources from '../i18n/en-US.json';
import React from 'react';
import { AuditKind } from '../dto/AuditKind';
import {
  CoreConstants,
  INotificationReact,
  NotificationMessageType
} from '@etsoo/react';
import { TabLayout } from '../dto/TabLayout';

/**
 * Service App
 */
class MyServiceApp extends CommonApp {
  private triedCount = 0;
  private loginDialog?: INotificationReact;

  private formatTitle(result: DynamicActionResult): [boolean, string] {
    let disabled: boolean = false;
    let title: string = result.title ?? 'Unknown';

    switch (result.type) {
      case 'UserFrozen':
        const frozenTime = new Date(result.data?.frozenTime);
        title = title.format(frozenTime.toLocaleString(this.culture));
        disabled = true;
        break;
    }

    return [disabled, title];
  }

  formatUrl(url: string) {
    url = url.toLowerCase().replace(/[^0-9a-z_]+/g, '');
    return url;
  }

  getLocalRoles() {
    return this.getRoles(UserRole.User | UserRole.Admin);
  }

  getAuditKinds() {
    return this.getEnumList(AuditKind, 'ak');
  }

  getTabLayouts() {
    return this.getEnumList(TabLayout, 'layout');
  }

  async translate(text: string) {
    return this.api.post<string>(
      'Article/Translate',
      { text },
      { showLoading: false }
    );
  }

  /**
   * Reset user password
   * @param id User id
   * @param callback Callback
   */
  async resetPassword(id: string, callback?: () => void) {
    const result = await this.api.put<IActionResult<{ password: string }>>(
      'User/ResetPassword/',
      { deviceId: this.deviceId, id }
    );
    if (result == null) return;

    if (result.ok && result.data) {
      var title =
        this.get<string>('newPassword') +
        ': ' +
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
      'etsooCMS',
      'login',
      'password',
      'unknownError',
      'user'
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
        username: 'string',
        password: 'string'
      });

      // Validate data
      if (username == null) {
        DomUtils.setFocus('username', form);
        return false;
      }

      if (password == null || !app.isValidPassword(password)) {
        DomUtils.setFocus('password', form);
        return false;
      }

      const data: LoginRQ = {
        id: this.encrypt(username),
        deviceId: this.deviceId,
        pwd: this.encrypt(this.hash(password)),
        region: this.region,
        timezone: this.getTimeZone()
      };

      const payload: IApiPayload<ServiceLoginResult, any> = {};
      const result = await this.api.post<ServiceLoginResult>(
        'Auth/Login',
        data,
        payload
      );

      if (result == null) {
        return false;
      }

      if (result.ok) {
        // Token
        const refreshToken = this.getResponseToken(payload.response);

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

        app.history.push('/home/');

        // Delay
        setTimeout(() => this.loginDialog?.dismiss(), 0);

        return true;
      } else if (result.type === 'DbConnectionFailed') {
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
        'okButton'
      ) as HTMLButtonElement;
      if (okButton) okButton.disabled = disabled;
      mRef.current?.setError(title);

      return false;
    };

    this.loginDialog = this.showInputDialog({
      title: labels.login,
      message: `${labels.etsooCMS} (${process.env.REACT_APP_VERSION})`,
      fullScreen: this.smDown,
      cancelButton: false,
      callback: async (form) => await loginCallback(form),
      inputs: (
        <VBox gap={1} width="100%">
          <TextFieldEx
            inputRef={(ref) => {
              Reflect.set(loginRef, 'current', ref);

              const userIdSaved = this.storage.getPersistedData<string>(
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
              event.currentTarget.closest('form')?.requestSubmit()
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
MUGlobal.textFieldVariant = 'standard';

const supportedCultures: DataTypes.CultureDefinition[] = [
  zhCN(zhCNResources),
  zhHK(zhHKResources),
  enUS(enUSResources)
];
const supportedRegions = ['CN'];

// External settings
const externalSettings = ExternalSettings.Create();
if (externalSettings == null) {
  throw new Error('No external settings');
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
  currentCulture: DomUtils.getCulture(supportedCultures, detectedCulture)!
};

/**
 * Application
 */
export const app = new MyServiceApp(settings, 'etsooCMS');

/**
 * Notifier provider
 */
export const NotifierProvider = MyServiceApp.notifierProvider;
