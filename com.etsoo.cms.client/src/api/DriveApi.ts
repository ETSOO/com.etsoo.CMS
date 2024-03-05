import {
  ApiResponseType,
  EntityApi,
  IApiPayload,
  ResultPayload,
  StringIdResultPayload
} from '@etsoo/appscript';
import { ReactAppType } from '@etsoo/materialui';
import { DataTypes } from '@etsoo/shared';
import { DriveQueryDto } from './dto/drive/DriveQueryDto';
import { DriveUpdateDto } from './dto/drive/DriveUpdateDto';
import { DriveQueryRQ } from './rq/drive/DriveQueryRQ';
import { DriveShareFileRQ } from './rq/drive/DriveShareFileRQ';

/**
 * Online drive API
 */
export class DriveApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   */
  constructor(app: ReactAppType) {
    super('Drive', app);
  }
  /**
   * Delete
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  delete(id: string, payload?: ResultPayload) {
    return this.deleteBase(id, payload);
  }

  /**
   * Download file
   * @param id File id
   * @param payload Payload
   * @returns Result
   */
  async downloadFile(
    id: string
  ): Promise<[ReadableStream, string] | undefined> {
    const payload: IApiPayload<ReadableStream> = {
      responseType: ApiResponseType.Stream
    };

    const result = await this.api.get(
      `${this.flag}/DownloadFile/${id}`,
      undefined,
      payload
    );

    if (result == null || payload.response == null) return;

    const filename =
      this.api.getContentDisposition(payload.response)?.filename ??
      'DownloadFile';

    return [result, filename];
  }

  /**
   * Query
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  query(rq: DriveQueryRQ, payload?: IApiPayload<DriveQueryDto[]>) {
    return this.queryBase(rq, payload);
  }

  /**
   * Share file
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  shareFile(rq: DriveShareFileRQ, payload?: StringIdResultPayload) {
    return this.api.put(`${this.flag}/ShareFile`, rq, payload);
  }

  /**
   * Update
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  update(
    rq: DataTypes.AddOrEditType<DriveUpdateDto, true>,
    payload?: ResultPayload
  ) {
    return this.updateBase(rq, payload);
  }

  /**
   * Upload files
   * @param files Files
   * @param payload Payload
   * @returns Result
   */
  uploadFiles(files: FileList, payload?: ResultPayload) {
    return this.api.post(`${this.flag}/UploadFiles`, files, payload);
  }
}
