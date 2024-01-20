import { Button } from "@mui/material";
import "./Post.css"
import FavoriteBorderIcon from '@mui/icons-material/FavoriteBorder';
import FavoriteIcon from '@mui/icons-material/Favorite';
import InsertCommentIcon from '@mui/icons-material/InsertComment';

export default function Post({ props })
{
    const { postId, postedBy, likes, content, mediaURL, timestamp, postedByPic } = props;
    

    return <div className="post_container">
        <div className="post_user">
            {postedBy}
            <img className="thumbnail" src={postedByPic}></img>
        </div>
        <img className="post_media" src={mediaURL}></img>
        <Button><FavoriteBorderIcon/></Button>
        <Button>{likes} Like{likes != 1?'s':''}</Button>
        <Button><InsertCommentIcon /></Button>
        {content}
    </div>
}