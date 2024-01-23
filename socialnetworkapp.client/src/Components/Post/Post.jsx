import { Button } from "@mui/material";
import "./Post.css"
import FavoriteBorderIcon from '@mui/icons-material/FavoriteBorder';
import FavoriteIcon from '@mui/icons-material/Favorite';
import InsertCommentIcon from '@mui/icons-material/InsertComment';
import DeleteIcon from '@mui/icons-material/Delete'
import { useState } from "react";
import { CURRENT_USER, postController, userController } from "../../Constants";
import { useNavigate,Link } from "react-router-dom";

export default function Post({ props,parentPostId})
{
    const navigate=useNavigate()
    const [postState,setPostState] = useState(props)
    const currentUser = JSON.parse(localStorage.getItem(CURRENT_USER))
    const isComment = postState.postId.startsWith('comment:')
    async function handleLike(ev)
    {
        const step = postState.liked ? -1 : 1
        const req = postState.postId.startsWith('post:') ? postController + `/LikePost/${currentUser.userId}/${postState.postId}`
            :postController+`/LikeComment/${currentUser.userId}/${parentPostId}/${postState.postId}`
        const res = await fetch(req, {
            method:'PUT'
        })
        if (!res.ok)
        {
            alert('Something went wrong.')
            return
            }
        setPostState(oldValue =>
        {
            return {
                ...oldValue,
                liked: !oldValue.liked,
                likes:oldValue.likes+step
            }
            })
    }
    async function displayLikes(ev)
    {
        navigate('/post/'+postState.postId+'/likes')
    }
    async function displayComments(ev)
    {
        navigate('/post/'+postState.postId+'/comments')
    }

    async function handleDelete(ev)
    {
        const request = await fetch(postController + `/DeletePost/${props.postId}/${currentUser.userId}`,
            {
            method:"DELETE"
        })
        if (request.ok)
        {
            location.reload()
            return
        }
        alert('uh oh')
    }

    return (!isComment?<div className="post_container">
        <div className="post_user">
        <img className="thumbnail" src={postState.postedByPic}></img>
            <Link to={`https://localhost:5173/profile/` + postState.postedById}>{postState.postedBy}</Link>

        </div>
        <img className="post_media" src={postState.mediaURL}></img>
        <div className="post_button_container">
            <Button onClick={handleLike}>{postState.liked ? <FavoriteIcon /> : <FavoriteBorderIcon />}</Button>
            <Button onClick={displayLikes}>{postState.likes} Like{postState.likes != 1?'s':''}</Button>
            <Button onClick={displayComments}><InsertCommentIcon /></Button>
            {postState.postedById === currentUser.userId && <Button onClick={handleDelete}><DeleteIcon /></Button>}
        </div>
       
        {postState.content}
    </div> :
        <div className="comment_container">
            <div className="post_user">
               
                <img className="thumbnail" src={postState.postedByPic}></img>
                <p>
                    {`${postState.postedBy}:     ${postState.content}`}
                </p>
            </div>
            <div className="comment_button_container">
                <Button onClick={handleLike}>{postState.liked ? <FavoriteIcon /> : <FavoriteBorderIcon />}</Button>
                <Button onClick={displayLikes}>{postState.likes} Like{postState.likes != 1?'s':''}</Button>
                <Button onClick={displayComments}><InsertCommentIcon /></Button>
    
        </div>
        </div>
    )
}