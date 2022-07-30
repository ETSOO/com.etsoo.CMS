import {
  zhCN,
  zhHK,
  enUS,
  ApiAuthorizationScheme,
  AddressUtils,
  ExternalSettings,
  IApiPayload,
  LoginRQ
} from '@etsoo/appscript';
import {
  IServiceAppSettings,
  MUGlobal,
  ServiceApp,
  ServiceLoginResult,
  TextFieldEx,
  VBox
} from '@etsoo/react';
import { DataTypes, DomUtils, Utils } from '@etsoo/shared';
import zhCNResources from '../i18n/zh-CN.json';
import zhHKResources from '../i18n/zh-HK.json';
import enUSResources from '../i18n/en-US.json';
import { MyUser } from './MyUser';
import React from 'react';

/**
 * Service App
 */
class MyServiceApp extends ServiceApp<MyUser> {
  /**
   * Go to the login page
   * @param tryLogin Try to login again
   */
  override toLoginPage(tryLogin?: boolean) {
    // Try login
    // Dialog to receive password
    var labels = this.getLabels('etsooCMS', 'login', 'password', 'user');

    const loginRef = React.createRef<HTMLInputElement>();
    const passwordRef = React.createRef<HTMLInputElement>();

    app.showInputDialog({
      title: labels.login,
      message: `${labels.etsooCMS} (${process.env.REACT_APP_VERSION})`,
      fullScreen: app.smDown,
      cancelButton: false,
      callback: async (form) => {
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

        if (password == null || password.length < 6) {
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
        const result = await app.api.post<ServiceLoginResult>(
          'Auth/Login',
          data,
          payload
        );

        if (result == null) {
          return false;
        }

        console.log(result);

        return false;
      },
      inputs: (
        <VBox gap={1} width="100%">
          <TextFieldEx
            inputRef={loginRef}
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
