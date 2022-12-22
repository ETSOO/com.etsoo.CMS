import { EntityApi, IApiPayload, IdResultPayload } from '@etsoo/appscript';
import { ReactAppType } from '@etsoo/materialui';
import { DataTypes } from '@etsoo/shared';
import { ArticleQueryDto } from './dto/article/ArticleQueryDto';
import { ArticleUpdateDto } from './dto/article/ArticleUpdateDto';
import { ArticleViewDto } from './dto/article/ArticleViewDto';
import { ArticleQueryRQ } from './rq/article/ArticleQueryRQ';

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
   * Query
   * @param rq Request data
   * @param payload Payload
   * @returns Result
   */
  query(rq: ArticleQueryRQ, payload?: IApiPayload<ArticleQueryDto[]>) {
    return this.queryBase(rq, payload);
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
   * Read for update
   * @param id Id
   * @param payload Payload
   * @returns Result
   */
  updateRead(id: number, payload?: IApiPayload<ArticleUpdateDto>) {
    return super.updateReadBase(id, payload);
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
