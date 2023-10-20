import {
  EntityApi,
  IApiPayload,
  IdResultPayload,
  ResultPayload
} from '@etsoo/appscript';
import { ReactAppType } from '@etsoo/materialui';
import { DataTypes } from '@etsoo/shared';
import { ArticleQueryDto } from './dto/article/ArticleQueryDto';
import { ArticleUpdateDto } from './dto/article/ArticleUpdateDto';
import { ArticleViewDto } from './dto/article/ArticleViewDto';
import { GalleryPhotoDto } from './dto/article/GalleryPhotoDto';
import { ArticleDeletePhotoRQ } from './rq/article/ArticleDeletePhotoRQ';
import { ArticleQueryRQ } from './rq/article/ArticleQueryRQ';
import { ArticleSortPhotosRQ } from './rq/article/ArticleSortPhotosRQ';
import { ArticleUpdatePhotoRQ } from './rq/article/ArticleUpdatePhotoRQ';

/**
 * Article API
 */
export class ArticleApi extends EntityApi {
  /**
   * Constructor
   * @param app Application
   */
  constructor(app: ReactAppType) {
    super('Article', app);
  }

  /**
   * Create
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  create(
    rq: DataTypes.AddOrEditType<ArticleUpdateDto, false>,
    payload?: IdResultPayload
  ) {
    return this.createBase(rq, payload);
  }

  /**
   * Delete
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  delete(id: number, payload?: ResultPayload) {
    return this.deleteBase(id, payload);
  }

  /**
   * Delete photo
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  deletePhoto(rq: ArticleDeletePhotoRQ, payload?: ResultPayload) {
    return this.api.put('Article/DeletePhoto', rq, payload);
  }

  /**
   * Query
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  query(rq: ArticleQueryRQ, payload?: IApiPayload<ArticleQueryDto[]>) {
    return this.queryBase(rq, payload);
  }

  /**
   * Sort gallery photos
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  sortPhotos(rq: ArticleSortPhotosRQ, payload?: ResultPayload) {
    return this.api.put('Article/SortPhotos', rq, payload);
  }

  /**
   * Translate text
   * @param text Source text
   * @param payload Payload
   * @returns Result
   */
  translate(text: string, payload?: IApiPayload<string>) {
    return this.api.post('Article/Translate', { text }, payload);
  }

  /**
   * Update
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  update(
    rq: DataTypes.AddOrEditType<ArticleUpdateDto, true>,
    payload?: IdResultPayload
  ) {
    return this.updateBase(rq, payload);
  }

  /**
   * Update photo
   * @param id Id
   * @param data Logo form data
   * @param payload Payload
   * @returns Result
   */
  updatePhoto(rq: ArticleUpdatePhotoRQ, payload?: ResultPayload) {
    return this.api.put(`${this.flag}/updatePhoto`, rq, payload);
  }

  /**
   * Upload logo
   * @param id Id
   * @param data Logo form data
   * @param payload Payload
   * @returns Result
   */
  uploadLogo(id: number, data: FormData, payload?: IApiPayload<string>) {
    return this.api.put(`${this.flag}/UploadLogo/${id}`, data, payload);
  }

  /**
   * Upload photos
   * @param id Profile id
   * @param files Files
   * @param payload Payload
   * @returns Result
   */
  uploadPhotos(id: number, files: FileList, payload?: ResultPayload) {
    return this.api.post(`${this.flag}/UploadPhotos/${id}`, files, payload);
  }

  /**
   * Read for update
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  updateRead(id: number, payload?: IApiPayload<ArticleUpdateDto>) {
    return super.updateReadBase(id, payload);
  }

  /**
   * View gallery photos
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  viewGallery(id: number, payload?: IApiPayload<GalleryPhotoDto[]>) {
    return this.api.get(`Article/ViewGallery/${id}`, undefined, payload);
  }

  /**
   * Read for view
   * @param id Id
   * @returns Result
   */
  viewRead(id: number, payload?: IApiPayload<ArticleViewDto>) {
    return this.api.get(`Article/ViewRead/${id}`, undefined, payload);
  }
}
